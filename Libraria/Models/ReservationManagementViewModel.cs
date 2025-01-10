namespace Libraria.Models
{
    public class ReservationManagementViewModel
    {
        public int ReservationID { get; set; }
        public DateTime ReservationDate { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string BookTitle { get; set; }
        public string BranchName { get; set; }
    }
}

