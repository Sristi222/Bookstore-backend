namespace Try_application.Model
{
    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }

        // New properties to handle discounts and final price
        public decimal DiscountAmount { get; set; } // Discount applied to this item
        public decimal FinalPrice { get; set; } // Final price after discount
    }
}
