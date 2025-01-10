namespace Libraria.Models
{
    public class Loan
    {
        public int LoanID { get; set; }
        public int UserID { get; set; }
        public int BookID { get; set; }
        public DateOnly BorrowDate { get; set; }
        public DateOnly? ReturnDate { get; set; }
        public int BranchID { get; set; }
    }
}
