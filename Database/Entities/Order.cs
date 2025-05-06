namespace Try_application.Database.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; }  // FK to User
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending";  // Pending, Confirmed, Shipped, Cancelled

        public ICollection<OrderItem> Items { get; set; }
    }
}
