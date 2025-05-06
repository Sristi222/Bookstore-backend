namespace Try_application.Database.Entities
{
    public class Product
    {
        public int Id { get; set; }

        public string? Name { get; set; }            // Book Title
        public string? Author { get; set; }
        public string? Genre { get; set; }
        public string? Publisher { get; set; }
        public string? ISBN { get; set; }

        public string? Description { get; set; }
        public string? Language { get; set; }
        public string? Format { get; set; }          // Paperback, Hardcover, etc.

        public DateTime? PublicationDate { get; set; }

        public decimal Price { get; set; }

        public decimal? DiscountPercent { get; set; }
        public DateTime? DiscountStartDate { get; set; }
        public DateTime? DiscountEndDate { get; set; }

        public bool OnSale { get; set; }

        public int StockQuantity { get; set; }
        public bool IsAvailableInStore { get; set; }

        public string? Image { get; set; }           // Cover Image Path

        public double Rating { get; set; }           // Average Rating
        public int TotalSold { get; set; }           // For popularity tracking
    }
}
