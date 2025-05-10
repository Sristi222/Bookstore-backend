namespace Try_application.Model
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ClaimCode { get; set; } // ✅ Added claim code
        public List<OrderItemDto> OrderItems { get; set; }
    }
}
