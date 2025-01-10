using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Libraria.Models;
using Microsoft.Data.SqlClient;
using System.Data;

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
                var loans = await connection.QueryAsync<LoanManagementViewModel>(
                    "GetLoanManagementData",
                    commandType: CommandType.StoredProcedure
                );
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
        [HttpGet]
        public async Task<IActionResult> ReservationManagement(string email = null)
        {
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                var reservations = await connection.QueryAsync<ReservationManagementViewModel>(
                    "GetReservationManagementData",
                    new { Email = email },
                    commandType: CommandType.StoredProcedure
                );
                return View(reservations);
            }
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelReservation(int reservationId)
        {
            if (reservationId <= 0)
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
                        // Delete the reservation from the database
                        var deleteReservationSql = @"
                            DELETE FROM Reservation
                            WHERE ReservationID = @ReservationID
                        ";

                        var rowsAffected = await connection.ExecuteAsync(
                            deleteReservationSql,
                            new { ReservationID = reservationId },
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

            return RedirectToAction("ReservationManagement");
        }
    }
}

