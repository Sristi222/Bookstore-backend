namespace Try_application.Database.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public decimal TotalAmount { get; set; } // Final total after discount
        public decimal DiscountAmount { get; set; } // Discount applied to the order
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OrderItem> OrderItems { get; set; }

        public string ClaimCode { get; set; } // Unique claim code for the order
    }
}
