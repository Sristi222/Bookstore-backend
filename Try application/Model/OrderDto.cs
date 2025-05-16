namespace Try_application.Model
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public decimal TotalAmount { get; set; } // Final price after discount
        public decimal DiscountAmount { get; set; } // Discount applied
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ClaimCode { get; set; } // Claim code for the order
        public List<OrderItemDto> OrderItems { get; set; } // List of items in the order
    }
}
