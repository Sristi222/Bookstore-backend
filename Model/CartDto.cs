using System.Collections.Generic;

namespace Try_application.Model
{
    public class CartDto
    {
        public List<CartItemDto> Items { get; set; } = new();
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }
        public int ItemCount { get; set; }
    }
}