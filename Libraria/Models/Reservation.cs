namespace Libraria.Models
{
    public class Reservation
    {
        public int ReservationID { get; set; }
        public int UserID { get; set; }
        public int BookID { get; set; }
        public DateOnly ReservationDate { get; set; }
        public int BranchID { get; set; }
    }
}
