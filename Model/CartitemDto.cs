using Try_application.Model;

namespace Try_application.Model
{
    public class CartItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public ProductDto Product { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
    }
}