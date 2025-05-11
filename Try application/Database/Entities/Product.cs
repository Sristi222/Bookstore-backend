using System;
using System.ComponentModel.DataAnnotations;

namespace Try_application.Database.Entities
{
    public class Product
    {
        public int Id { get; set; }

        // Basic Info
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }
        public string Image { get; set; }
        public string Author { get; set; }
        public string Genre { get; set; }
        public string Publisher { get; set; }
        public string ISBN { get; set; }
        public string Language { get; set; }
        public string Format { get; set; }

        // Dates
        public DateTime? PublicationDate { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.UtcNow;

        // Pricing and Sale Info
        [Required]
        public decimal Price { get; set; }

        public decimal? DiscountPercent { get; set; }
        public DateTime? DiscountStartDate { get; set; }
        public DateTime? DiscountEndDate { get; set; }
        public bool OnSale { get; set; }

        // Inventory
        public int StockQuantity { get; set; }
        public bool IsAvailableInStore { get; set; }

        // Sales and Ratings
        public int TotalSold { get; set; } = 0;
        public decimal Rating { get; set; } = 0;

        // Category Flags
        public bool HasAward { get; set; } = false;
        public bool IsTrending { get; set; } = false;
        public bool IsBestseller { get; set; } = false;
        public bool IsNewRelease { get; set; } = false;
        public bool IsComingSoon { get; set; } = false;
        public bool IsOnDeal { get; set; } = false;
    }
}
