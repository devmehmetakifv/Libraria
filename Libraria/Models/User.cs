namespace Libraria.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public DateTime MembershipDate { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }
}
