using System.ComponentModel.DataAnnotations;

namespace Try_application.Model
{
    public class BookDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(150)]
        public string? Author { get; set; }

        [StringLength(100)]
        public string? Genre { get; set; }

        [Required]
        [StringLength(150)]
        public string Publisher { get; set; } = string.Empty;

        [StringLength(20)]
        public string? ISBN { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? Language { get; set; }

        [StringLength(50)]
        public string? Format { get; set; }

        [Required]
        public DateTime PublicationDate { get; set; }

        [Required]
        [Range(0, 100000)]
        public decimal Price { get; set; }

        [Range(0, 100)]
        public decimal? DiscountPercent { get; set; }

        public DateTime? DiscountStartDate { get; set; }

        public DateTime? DiscountEndDate { get; set; }

        public bool OnSale { get; set; }

        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        public bool IsAvailableInStore { get; set; }

        [StringLength(500)]
        public string? CoverImageUrl { get; set; }
    }
}
