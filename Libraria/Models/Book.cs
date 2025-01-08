namespace Libraria.Models
{
    public class Book
    {
        public int BookID { get; set; }
        public string Title { get; set; }
        public string ISBN { get; set; }
        public DateTime PublicationDate { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryID { get; set; }
    }
}
