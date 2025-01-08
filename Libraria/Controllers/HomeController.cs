using System.Data.SqlClient;
using System.Diagnostics;
using Dapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Libraria.Models;
using Microsoft.Data.SqlClient;

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
                MAX(r.ReservationDate) AS ReservationDate
            FROM 
                Book b
            LEFT JOIN 
                Reservation r ON b.BookID = r.BookID
            GROUP BY 
                b.BookID, b.Title, b.ISBN, b.PublicationDate, b.StockQuantity, b.CategoryID
        ";

                var books = await connection.QueryAsync<dynamic>(sql);
                return Json(books);
            }
        }

    }
}
