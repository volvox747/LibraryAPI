namespace LibraryAPI.Models
{
    public class BookModel
    {
        public int BookId { get; set; }
        public string BookName { get; set; }
        public string Author { get; set; }
        public string PublishDate { get; set; }
        public int Quantity { get; set; }
        public string Category { get; set; }
        public string Language { get; set; }
        public string Description { get; set; }
        public string BookImageUrl { get; set; }
    }
}
