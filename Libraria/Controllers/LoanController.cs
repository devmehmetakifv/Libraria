using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Libraria.Models;
using Microsoft.Data.SqlClient;

namespace Libraria.Controllers
{
    [Authorize(Roles = "Admin")]
    public class LoanController : Controller
    {
        private readonly IConfiguration _configuration;

        public LoanController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> LoanManagement()
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                // Combine the creation of the temporary table, the MERGE statement, and the DELETE statement into a single SQL batch
                var updateFinesSql = @"
                    -- Create a temporary table to store overdue loans
                    CREATE TABLE #OverdueLoans (
                        LoanID INT,
                        OverdueDays INT
                    );

                    -- Insert overdue loans into the temporary table
                    INSERT INTO #OverdueLoans (LoanID, OverdueDays)
                    SELECT 
                        l.LoanID,
                        DATEDIFF(day, l.BorrowDate, ISNULL(l.ReturnDate, GETDATE())) - 15 AS OverdueDays
                    FROM Loan l
                    WHERE DATEDIFF(day, l.BorrowDate, ISNULL(l.ReturnDate, GETDATE())) > 15;

                    -- Update fines for all overdue loans using the temporary table
                    MERGE Fine AS target
                    USING #OverdueLoans AS source
                    ON target.LoanID = source.LoanID
                    WHEN MATCHED THEN 
                        UPDATE SET FineAmount = source.OverdueDays
                    WHEN NOT MATCHED THEN
                        INSERT (LoanID, FineAmount)
                        VALUES (source.LoanID, source.OverdueDays);

                    -- Delete fines for loans that are no longer overdue
                    DELETE FROM Fine
                    WHERE LoanID NOT IN (SELECT LoanID FROM #OverdueLoans);

                    -- Drop the temporary table
                    DROP TABLE #OverdueLoans;
                ";

                await connection.ExecuteAsync(updateFinesSql);

                // Retrieve loan data along with fine amounts
                var sql = @"
                    SELECT 
                        l.LoanID,
                        l.UserID,
                        u.Email AS UserEmail,
                        l.BookID,
                        b.Title AS BookTitle,
                        l.BorrowDate,
                        l.ReturnDate,
                        l.BranchID,
                        lb.BranchName,
                        f.FineAmount
                    FROM 
                        Loan l
                    INNER JOIN 
                        UserTable u ON l.UserID = u.UserID
                    INNER JOIN 
                        Book b ON l.BookID = b.BookID
                    LEFT JOIN 
                        LibraryBranch lb ON l.BranchID = lb.BranchID
                    LEFT JOIN
                        Fine f ON l.LoanID = f.LoanID
                    ORDER BY 
                        l.BorrowDate DESC
                ";

                var loans = await connection.QueryAsync<LoanManagementViewModel>(sql);
                return View(loans);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsReturned(int loanId)
        {
            if (loanId <= 0)
            {
                return BadRequest();
            }

            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Delete related fines first
                        var deleteFineSql = @"
                            DELETE FROM Fine
                            WHERE LoanID = @LoanID
                        ";

                        await connection.ExecuteAsync(
                            deleteFineSql,
                            new { LoanID = loanId },
                            transaction
                        );

                        // Delete the loan from the database
                        var deleteLoanSql = @"
                            DELETE FROM Loan
                            WHERE LoanID = @LoanID
                        ";

                        var rowsAffected = await connection.ExecuteAsync(
                            deleteLoanSql,
                            new { LoanID = loanId },
                            transaction
                        );

                        if (rowsAffected == 0)
                        {
                            transaction.Rollback();
                            return NotFound();
                        }

                        // Commit the transaction
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        // Log the exception if necessary
                        return StatusCode(500, "Internal server error");
                    }
                }
            }

            return RedirectToAction("LoanManagement");
        }
    }
}
