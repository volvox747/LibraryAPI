namespace LibraryAPI.Models
{
    public class RequestBooksModel
    {
        public string ReqId { get; set; }
        public string BookId { get; set; }
        public string BookName { get; set; }
        public string BookImageUrl { get; set; }
        public string Author { get; set; }
        public string Category { get; set; }
        public string LangId { get; set; }
        public string Language { get; set; }
        public string LoginId { get; set; }
        public string LoginEmail { get; set; }
        public string PhoneNumber { get; set; }
        public string Status { get; set; }
    }
}
