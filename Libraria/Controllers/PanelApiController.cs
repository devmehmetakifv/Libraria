using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Libraria.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PanelApiController : ControllerBase
    {
        private readonly string _connectionString = "Server=.\\sqlexpress;Database=LibraryManagementDatabase;Trusted_Connection=True;TrustServerCertificate=True;";

        [HttpGet("HasLoans/{bookId}")]
        public async Task<IActionResult> HasLoans(int bookId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                const string query = "SELECT COUNT(1) FROM Loan WHERE BookID = @BookID";
                var hasLoans = await connection.ExecuteScalarAsync<int>(query, new { BookID = bookId });
                return Ok(hasLoans > 0);
            }
        }
    }
}
