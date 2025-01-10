namespace Libraria.Models
{
    public class ReservationViewModel
    {
        public int ReservationID { get; set; }
        public string Title { get; set; }
        public DateTime ReservationDate { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string? BranchName { get; set; }
    }
}
