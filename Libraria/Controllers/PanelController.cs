using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Libraria.Models;
using Microsoft.Data.SqlClient;
using Dapper;

namespace Libraria.Controllers
{
    [Authorize]
    public class PanelController : Controller
    {
        private readonly string _connectionString = "Server=.\\sqlexpress;Database=LibraryManagementDatabase;Trusted_Connection=True;TrustServerCertificate=True;";

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
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UserManagement()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var users = await connection.QueryAsync<User>("SELECT UserID, Name, Surname, Email, MembershipDate, Role FROM UserTable");
                var admins = await connection.QueryAsync<Admin>("SELECT AdminID, Name, Surname, Email, Role, BranchID FROM AdminTable");
                var branches = await connection.QueryAsync<LibraryBranch>("SELECT BranchID, BranchName FROM LibraryBranch");

                var viewModel = new AdminUserBranchViewModel
                {
                    Users = users.ToList(),
                    Admins = admins.ToList(),
                    Branches = branches.ToList()
                };
                return View(viewModel);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddNewAdmin(string name, string surname, string email, string password, int branchId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var parameters = new { Name = name, Surname = surname, Email = email, Password = password, Role = "Admin", BranchID = branchId };
                int rowsAffected = await connection.ExecuteAsync("INSERT INTO AdminTable (Name, Surname, Email, Password, Role, BranchID) VALUES (@Name, @Surname, @Email, @Password, @Role, @BranchID)", parameters);

                if (rowsAffected > 0)
                {
                    TempData["SuccessMessage"] = "Admin registered successfully!";
                    return RedirectToAction("UserManagement");
                }
                else
                {
                    TempData["ErrorMessage"] = "Admin could not be created!";
                    return RedirectToAction("UserManagement");
                }
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddNewUser(string name, string surname, string email, string password)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var parameters = new { Name = name, Surname = surname, Email = email, MembershipDate = DateOnly.FromDateTime(DateTime.Now), Password = password, Role = "User" };
                int rowsAffected = await connection.ExecuteAsync("INSERT INTO UserTable (Name, Surname, Email, MembershipDate, Password, Role) VALUES (@Name, @Surname, @Email, @MembershipDate, @Password, @Role)", parameters);

                if (rowsAffected > 0)
                {
                    TempData["SuccessMessage"] = "User registered successfully!";
                    return RedirectToAction("UserManagement");
                }
                else
                {
                    TempData["ErrorMessage"] = "User could not be created!";
                    return RedirectToAction("UserManagement");
                }
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BringUser(string name, string surname)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var user = await connection.QueryFirstOrDefaultAsync<User>("SELECT UserID, Name, Surname, Email FROM UserTable WHERE Name = @Name AND Surname = @Surname", new { Name = name, Surname = surname });

                if (user != null)
                {
                    return Json(new { success = true, data = user });
                }
                else
                {
                    return Json(new { success = false, data = "User not found!" });
                }
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> EditUser(int userId, string name, string surname, string email)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var parameters = new { UserID = userId, Name = name, Surname = surname, Email = email };
                int rowsAffected = await connection.ExecuteAsync("UPDATE UserTable SET Name = @Name, Surname = @Surname, Email = @Email WHERE UserID = @UserID", parameters);

                if (rowsAffected > 0)
                {
                    TempData["SuccessMessage"] = "User updated successfully!";
                    return RedirectToAction("UserManagement");
                }
                else
                {
                    TempData["ErrorMessage"] = "User could not be updated or user not found!";
                    return RedirectToAction("UserManagement");
                }
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> DeleteUser(int deleteUserId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                int rowsAffected = await connection.ExecuteAsync("DELETE FROM UserTable WHERE UserID = @UserID", new { UserID = deleteUserId });

                if (rowsAffected > 0)
                {
                    TempData["SuccessMessage"] = "User deleted successfully!";
                    return RedirectToAction("UserManagement");
                }
                else
                {
                    TempData["ErrorMessage"] = "User could not be deleted or user not found!";
                    return RedirectToAction("UserManagement");
                }
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BookManagement()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var books = await connection.QueryAsync<Book>("SELECT BookID, Title, ISBN, PublicationDate, StockQuantity, CategoryID FROM Book");
                var categories = await connection.QueryAsync<Category>("SELECT CategoryID, CategoryName FROM Category");

                var viewModel = new BookCategoryViewModel
                {
                    Books = books.ToList(),
                    Categories = categories.ToList()
                };
                return View(viewModel);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddNewBook(string title, string isbn, DateOnly publicationDate, int stockQuantity, int categoryId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var parameters = new { Title = title, ISBN = isbn, PublicationDate = publicationDate, StockQuantity = stockQuantity, CategoryID = categoryId };
                int rowsAffected = await connection.ExecuteAsync("INSERT INTO Book (Title, ISBN, PublicationDate, StockQuantity, CategoryID) VALUES (@Title, @ISBN, @PublicationDate, @StockQuantity, @CategoryID)", parameters);

                if (rowsAffected > 0)
                {
                    TempData["SuccessMessage"] = "Book added successfully!";
                    return RedirectToAction("BookManagement");
                }
                else
                {
                    TempData["ErrorMessage"] = "Book could not be added!";
                    return RedirectToAction("BookManagement");
                }
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BringBook(string bookTitle)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var book = await connection.QueryFirstOrDefaultAsync<Book>("SELECT BookID, Title, ISBN, PublicationDate, StockQuantity, CategoryID FROM Book WHERE Title = @Title", new { Title = bookTitle });

                if (book != null)
                {
                    return Json(new { success = true, data = book });
                }
                else
                {
                    return Json(new { success = false, data = "Book not found!" });
                }
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> EditBook(int bookId, string title, string isbn, DateOnly publicationDate, int stockQuantity, int categoryId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var parameters = new { BookID = bookId, Title = title, ISBN = isbn, PublicationDate = publicationDate, StockQuantity = stockQuantity, CategoryID = categoryId };
                int rowsAffected = await connection.ExecuteAsync("UPDATE Book SET Title = @Title, ISBN = @ISBN, PublicationDate = @PublicationDate, StockQuantity = @StockQuantity, CategoryID = @CategoryID WHERE BookID = @BookID", parameters);

                if (rowsAffected > 0)
                {
                    TempData["SuccessMessage"] = "Book updated successfully!";
                    return RedirectToAction("BookManagement");
                }
                else
                {
                    TempData["ErrorMessage"] = "Book could not be updated or book not found!";
                    return RedirectToAction("BookManagement");
                }
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> DeleteBook(int deleteBookId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                int rowsAffected = await connection.ExecuteAsync("DELETE FROM Book WHERE BookID = @BookID", new { BookID = deleteBookId });

                if (rowsAffected > 0)
                {
                    TempData["SuccessMessage"] = "Book deleted successfully!";
                    return RedirectToAction("BookManagement");
                }
                else
                {
                    TempData["ErrorMessage"] = "Book could not be deleted or book not found!";
                    return RedirectToAction("BookManagement");
                }
            }
        }
    }

    // --- View Models ---
    public class AdminUserBranchViewModel
    {
        public List<User> Users { get; set; }
        public List<Admin> Admins { get; set; }
        public List<LibraryBranch> Branches { get; set; }
    }

    public class BookCategoryViewModel
    {
        public List<Book> Books { get; set; }
        public List<Category> Categories { get; set; }
    }
}