namespace Libraria.Models
{
    public class ManageReservationsAndLoansViewModel
    {
        public IEnumerable<ReservationViewModel> Reservations { get; set; }
        public IEnumerable<LoanViewModel> Loans { get; set; }
    }
}
