using System.Data.SqlClient;
using System.Diagnostics;
using Dapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Libraria.Models;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Libraria.Models;

namespace Libraria.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public async Task<IActionResult> GetBooks()
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                var sql = @"
            SELECT 
                b.BookID,
                b.Title,
                b.ISBN,
                b.PublicationDate,
                b.StockQuantity,
                c.CategoryName,
                ISNULL(r.CurrentReservations, 0) AS CurrentReservations,
                ISNULL(a.AuthorNames, 'N/A') AS AuthorNames,
                ISNULL(g.GenreNames, 'N/A') AS GenreNames
            FROM 
                Book b
            LEFT JOIN 
                (SELECT BookID, COUNT(*) AS CurrentReservations FROM Reservation GROUP BY BookID) r 
                ON b.BookID = r.BookID
            LEFT JOIN
                Category c ON b.CategoryID = c.CategoryID
            LEFT JOIN
                (
                    SELECT 
                        ba.BookID, 
                        STRING_AGG(au.FirstName + ' ' + au.LastName, ', ') AS AuthorNames
                    FROM 
                        BookAuthor ba
                    INNER JOIN 
                        Author au ON ba.AuthorID = au.AuthorID
                    GROUP BY 
                        ba.BookID
                ) a ON b.BookID = a.BookID
            LEFT JOIN
                (
                    SELECT 
                        bg.BookID, 
                        STRING_AGG(g.GenreName, ', ') AS GenreNames
                    FROM 
                        BookGenre bg
                    INNER JOIN 
                        Genre g ON bg.GenreID = g.GenreID
                    GROUP BY 
                        bg.BookID
                ) g ON b.BookID = g.BookID
        ";

                var books = await connection.QueryAsync<dynamic>(sql);
                return Json(books);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> ReserveBook([FromBody] ReserveBookModel model)
        {
            if (model == null || model.BookId <= 0)
            {
                return BadRequest();
            }

            // Get the current user's ID
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim.Value);

            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                // Check current reservations and stock quantity
                var checkSql = @"
                    SELECT 
                        b.StockQuantity,
                        COUNT(r.ReservationID) AS CurrentReservations
                    FROM 
                        Book b
                    LEFT JOIN 
                        Reservation r ON b.BookID = r.BookID
                    WHERE 
                        b.BookID = @BookID
                    GROUP BY 
                        b.StockQuantity
                ";

                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(checkSql, new { BookID = model.BookId });

                if (result == null)
                {
                    return NotFound("Book not found.");
                }

                int stockQuantity = result.StockQuantity;
                int currentReservations = result.CurrentReservations;

                if (currentReservations >= stockQuantity)
                {
                    return BadRequest("No more copies available for reservation.");
                }

                // Proceed with reservation
                var sql = @"
                    INSERT INTO Reservation (UserID, BookID, ReservationDate)
                    VALUES (@UserID, @BookID, @ReservationDate)
                ";

                var parameters = new
                {
                    UserID = userId,
                    BookID = model.BookId,
                    ReservationDate = DateTime.UtcNow
                };

                try
                {
                    await connection.ExecuteAsync(sql, parameters);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while reserving book.");
                    return StatusCode(500, "Internal server error");
                }
            }

            return Ok();
        }

        [HttpGet]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> ManageReservations()
        {
            // Get the current user's ID
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim.Value);

            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                // Retrieve Reservations
                var reservationsSql = @"
            SELECT 
                r.ReservationID,
                b.Title,
                r.ReservationDate
            FROM 
                Reservation r
            INNER JOIN 
                Book b ON r.BookID = b.BookID
            WHERE 
                r.UserID = @UserID
            ORDER BY 
                r.ReservationDate DESC
        ";

                var reservations = await connection.QueryAsync<ReservationViewModel>(reservationsSql, new { UserID = userId });

                // Retrieve Loans with FineAmount
                var loansSql = @"
            SELECT 
                l.LoanID,
                b.Title,
                l.BorrowDate,
                l.ReturnDate,
                f.FineAmount
            FROM 
                Loan l
            INNER JOIN 
                Book b ON l.BookID = b.BookID
            LEFT JOIN 
                Fine f ON l.LoanID = f.LoanID
            WHERE 
                l.UserID = @UserID
            ORDER BY 
                l.BorrowDate DESC
        ";

                var loans = await connection.QueryAsync<LoanViewModel>(loansSql, new { UserID = userId });

                // Create the composite ViewModel
                var viewModel = new ManageReservationsAndLoansViewModel
                {
                    Reservations = reservations,
                    Loans = loans
                };

                return View(viewModel);
            }
        }


        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> CancelReservation(int reservationId)
        {
            if (reservationId <= 0)
            {
                return BadRequest();
            }

            // Get the current user's ID
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim.Value);

            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                var sql = @"
                    DELETE FROM Reservation
                    WHERE ReservationID = @ReservationID AND UserID = @UserID
                ";

                try
                {
                    var rowsAffected = await connection.ExecuteAsync(sql, new { ReservationID = reservationId, UserID = userId });
                    if (rowsAffected == 0)
                    {
                        return NotFound();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while cancelling reservation.");
                    return StatusCode(500, "Internal server error");
                }
            }

            return RedirectToAction("ManageReservations");
        }
    }

    public class ReserveBookModel
    {
        public int BookId { get; set; }
    }

  
}
