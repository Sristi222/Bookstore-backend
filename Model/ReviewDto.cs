namespace Try_application.Model
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; } // optional for displaying
        public int ProductId { get; set; }
        public string Comment { get; set; }
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }

        public string ProductName { get; set; }
        public string ProductImage { get; set; }
    }
}
