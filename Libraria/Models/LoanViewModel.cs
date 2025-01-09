namespace Libraria.Models
{
    public class LoanViewModel
    {
        public int LoanID { get; set; }
        public string Title { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime? ReturnDate { get; set; } 
        public decimal? FineAmount { get; set; } 
    }
}
