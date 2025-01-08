using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Libraria.Controllers
{
    public class AccountController : Controller
    {
        private readonly string _connectionString;

        public AccountController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // GET: Account/Login
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                // Check in AdminTable
                var adminUser = await connection.QueryFirstOrDefaultAsync<User>(
                    "SELECT AdminID, Email FROM AdminTable WHERE Email = @Email AND Password = @Password",
                    new { Email = email, Password = password });

                if (adminUser != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, adminUser.Email),
                        new Claim(ClaimTypes.Role, "Admin"),
                        new Claim(ClaimTypes.NameIdentifier, adminUser.UserID.ToString())
                    };

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity));

                    return RedirectToAction("Index", "Home");
                }

                // Check in UserTable
                var user = await connection.QueryFirstOrDefaultAsync<User>(
                    "SELECT UserID, Email FROM UserTable WHERE Email = @Email AND Password = @Password",
                    new { Email = email, Password = password });

                if (user != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Email),
                        new Claim(ClaimTypes.Role, "User"),
                        new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString())
                    };

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity));

                    return RedirectToAction("Index", "Home");
                }

                ViewBag.Message = "Invalid email or password.";
                return View();
            }
        }

        // GET: Account/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login", "Account");
        }

        // GET: Account/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
    public class User
{
    public int UserID { get; set; } 
    public string Email { get; set; }
}

