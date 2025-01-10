using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Libraria.Models;
using Microsoft.Data.SqlClient;
using Dapper;

namespace Libraria.Controllers
{
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

        public async Task<IActionResult> UserManagement()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var users = await connection.QueryAsync<Libraria.Models.User>("SELECT UserID, Name, Surname, Email, MembershipDate, Role FROM UserTable");
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

        [HttpPost]
        public async Task<IActionResult> AddNewAdmin(string name, string surname, string email, string password, string confirmPassword, int branchId)
        {
            if (password != confirmPassword)
            {
                TempData["ErrorMessage"] = "Passwords do not match!";
                return RedirectToAction("UserManagement");
            }
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

        [HttpPost]
        public async Task<IActionResult> AddNewUser(string name, string surname, string email, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                TempData["ErrorMessage"] = "Passwords do not match!";
                return RedirectToAction("UserManagement");
            }
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var parameters = new { Name = name, Surname = surname, Email = email, MembershipDate = DateTime.Now, Password = password, Role = "User" };
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

        public async Task<IActionResult> BringUser(string email)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var user = await connection.QueryFirstOrDefaultAsync<User>("SELECT UserID, Name, Surname, Email FROM UserTable WHERE Email = @Email", new { Email = email });

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
        [HttpPost]
        public async Task<IActionResult> DeleteAdmin(int deleteAdminId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                int rowsAffected = await connection.ExecuteAsync("DELETE FROM AdminTable WHERE AdminID = @AdminID", new { AdminID = deleteAdminId });

                if (rowsAffected > 0)
                {
                    TempData["SuccessMessage"] = "Admin deleted successfully!";
                    return RedirectToAction("UserManagement");
                }
                else
                {
                    TempData["ErrorMessage"] = "Admin could not be deleted or user not found!";
                    return RedirectToAction("UserManagement");
                }
            }
        }
        [HttpGet("Panel/BookManagement")]
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

        [HttpPost]
        public async Task<IActionResult> AddNewBook(
            string bookTitle,
            string bookISBN,
            DateOnly bookPublicationDate,
            int bookStockQuantity,
            string bookCategoryName,
            string bookAuthorFirstName,
            string bookAuthorLastName,
            DateOnly bookAuthorBirthDate)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Convert DateOnly to DateTime
                        DateTime publicationDate = bookPublicationDate.ToDateTime(TimeOnly.MinValue);
                        DateTime authorBirthDate = bookAuthorBirthDate.ToDateTime(TimeOnly.MinValue);

                        // Check if the category exists
                        var categoryId = await connection.ExecuteScalarAsync<int?>(
                            "SELECT CategoryID FROM Category WHERE CategoryName = @CategoryName",
                            new { CategoryName = bookCategoryName },
                            transaction);

                        // If the category doesn't exist, insert it
                        if (categoryId == null)
                        {
                            categoryId = await connection.ExecuteScalarAsync<int>(
                                "INSERT INTO Category (CategoryName) OUTPUT INSERTED.CategoryID VALUES (@CategoryName)",
                                new { CategoryName = bookCategoryName },
                                transaction);
                        }

                        // Check if the author exists
                        var authorId = await connection.ExecuteScalarAsync<int?>(
                            "SELECT AuthorID FROM Author WHERE FirstName = @FirstName AND LastName = @LastName AND BirthDate = @BirthDate",
                            new { FirstName = bookAuthorFirstName, LastName = bookAuthorLastName, BirthDate = authorBirthDate },
                            transaction);

                        // If the author doesn't exist, insert them
                        if (authorId == null)
                        {
                            authorId = await connection.ExecuteScalarAsync<int>(
                                "INSERT INTO Author (FirstName, LastName, BirthDate) OUTPUT INSERTED.AuthorID VALUES (@FirstName, @LastName, @BirthDate)",
                                new { FirstName = bookAuthorFirstName, LastName = bookAuthorLastName, BirthDate = authorBirthDate },
                                transaction);
                        }

                        // Insert the book
                        var bookId = await connection.ExecuteScalarAsync<int>(
                            "INSERT INTO Book (Title, ISBN, PublicationDate, StockQuantity, CategoryID) OUTPUT INSERTED.BookID VALUES (@Title, @ISBN, @PublicationDate, @StockQuantity, @CategoryID)",
                            new
                            {
                                Title = bookTitle,
                                ISBN = bookISBN,
                                PublicationDate = publicationDate,
                                StockQuantity = bookStockQuantity,
                                CategoryID = categoryId
                            },
                            transaction);

                        // Link the book and the author in the BookAuthor table
                        await connection.ExecuteAsync(
                            "INSERT INTO BookAuthor (BookID, AuthorID) VALUES (@BookID, @AuthorID)",
                            new { BookID = bookId, AuthorID = authorId },
                            transaction);

                        transaction.Commit();

                        TempData["SuccessMessage"] = "Book added successfully!";
                        return RedirectToAction("BookManagement");
                    }
                    catch
                    {
                        transaction.Rollback();
                        TempData["ErrorMessage"] = "An error occurred while adding the book!";
                        return RedirectToAction("BookManagement");
                    }
                }
            }
        }


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

        [HttpPost]
        public async Task<IActionResult> EditBook(int bookId, string bookTitle, string bookISBN, DateTime bookPublicationDate, int bookStockQuantity, string bookCategoryName)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var bookCategoryId = await connection.QueryFirstOrDefaultAsync<int>("SELECT CategoryID FROM Category WHERE CategoryName = @CategoryName", new { CategoryName = bookCategoryName });
                var parameters = new { BookID = bookId, Title = bookTitle, ISBN = bookISBN, PublicationDate = bookPublicationDate, StockQuantity = bookStockQuantity, CategoryID = bookCategoryId };
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
        public async Task<IActionResult> GetCategories()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var categories = await connection.QueryAsync<Category>("SELECT CategoryID, CategoryName FROM Category");
                if (categories != null)
                {
                    return Json(new { success = true, data = categories });
                }
                else
                {
                    return Json(new { success = false, data = "Categories not found!" });
                }
            }
        }
        public async Task<IActionResult> GetGenres()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var genres = await connection.QueryAsync<Genre>("SELECT GenreID, GenreName FROM Genre");

                if (genres != null)
                {
                    return Json(new { success = true, data = genres });
                }
                else
                {
                    return Json(new { success = false, data = "Genres not found!" });
                }
            }
        }
        public async Task<IActionResult> GetAuthors()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var authors = await connection.QueryAsync<Author>("SELECT AuthorID, FirstName, LastName, BirthDate FROM Author");

                if (authors != null)
                {
                    return Json(new { success = true, data = authors });
                }
                else
                {
                    return Json(new { success = false, data = "Authors not found!" });
                }
            }
        }
    }
}