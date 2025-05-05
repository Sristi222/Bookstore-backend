namespace Try_application.Database.Entities
{
    public class Book
    {
        public int Id { get; set; }

        public required string Title { get; set; }
        public string? Author { get; set; }
        public string? Genre { get; set; }
        public required string Publisher { get; set; }
        public string? ISBN { get; set; }
        public string? Description { get; set; }
        public string? Language { get; set; }
        public string? Format { get; set; }

        public DateTime PublicationDate { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountPercent { get; set; }
        public DateTime? DiscountStartDate { get; set; }
        public DateTime? DiscountEndDate { get; set; }
        public bool OnSale { get; set; }
        public int StockQuantity { get; set; }
        public bool IsAvailableInStore { get; set; }
        public string? CoverImageUrl { get; set; }

        public double? Rating { get; set; }
        public int TotalSold { get; set; }
    }
}
