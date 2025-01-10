namespace Libraria.Models
{
    public class LoanManagementViewModel
    {
        public int LoanID { get; set; }
        public int UserID { get; set; }
        public string UserEmail { get; set; }
        public int BookID { get; set; }
        public string BookTitle { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public int? BranchID { get; set; }
        public string BranchName { get; set; }
        public decimal? FineAmount { get; set; }
    }
}
