namespace Try_application.Model
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string? Image { get; set; }
        public string? Name { get; set; }
        public string? Author { get; set; }
        public string? Genre { get; set; }
        public string? Publisher { get; set; }
        public string? ISBN { get; set; }
        public string? Description { get; set; }
        public string? Language { get; set; }
        public string? Format { get; set; }  // e.g. Paperback, Hardcover, Collector's
        public DateTime? PublicationDate { get; set; }

        public decimal Price { get; set; }

        public decimal? DiscountPercent { get; set; }
        public DateTime? DiscountStartDate { get; set; }
        public DateTime? DiscountEndDate { get; set; }

        public bool OnSale { get; set; }

        public int StockQuantity { get; set; }
        public bool IsAvailableInStore { get; set; }
    }
}
