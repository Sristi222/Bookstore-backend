namespace Try_application.Database.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }

        // New properties for discount and final price
        public decimal DiscountAmount { get; set; } // Discount applied to the item
        public decimal FinalPrice { get; set; } // Final price after discount
    }
}
