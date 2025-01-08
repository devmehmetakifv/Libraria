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
                        b.CategoryID,
                        r.ReservationDate
                    FROM 
                        Book b
                    LEFT JOIN 
                        (SELECT BookID, MAX(ReservationDate) AS ReservationDate FROM Reservation GROUP BY BookID) r 
                        ON b.BookID = r.BookID
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

        // Removed GetBranchIdForUser method since it's no longer needed

    }

    public class ReserveBookModel
    {
        public int BookId { get; set; }
    }
}
