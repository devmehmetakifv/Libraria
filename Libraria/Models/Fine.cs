namespace Libraria.Models
{
    public class Fine
    {
        public int FineID { get; set; }
        public int LoanID { get; set; }
        public int FineAmount { get; set; }
        public DateOnly FineDate { get; set; }
    }
}
