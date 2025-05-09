using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Try_application.Database;
using Try_application.Database.Entities;
using Try_application.Model; // for DTOs
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Try_application.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly AppDBContext _context;

        public OrdersController(AppDBContext context)
        {
            _context = context;
        }

        // ✅ POST: Place a new order from cart
        [HttpPost]
        public async Task<ActionResult<OrderDto>> PlaceOrder([FromQuery] string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("UserId is required.");

            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
                return BadRequest("Cart is empty.");

            var order = new Order
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                Status = "Pending",
                TotalAmount = cartItems.Sum(c => c.Product.Price * c.Quantity),
                OrderItems = cartItems.Select(c => new OrderItem
                {
                    ProductId = c.ProductId,
                    UnitPrice = c.Product.Price,
                    Quantity = c.Quantity
                }).ToList()
            };

            _context.Orders.Add(order);

            // Clear the cart after placing order
            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            // map to DTO
            var dto = new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                CreatedAt = order.CreatedAt,
                OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    UnitPrice = oi.UnitPrice,
                    Quantity = oi.Quantity
                }).ToList()
            };

            return Ok(dto);
        }

        // ✅ GET: List all orders for a user
        [HttpGet]
        public async Task<ActionResult<List<OrderDto>>> GetOrders([FromQuery] string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("UserId is required.");

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
                .ToListAsync();

            var orderDtos = orders.Select(o => new OrderDto
            {
                Id = o.Id,
                UserId = o.UserId,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                CreatedAt = o.CreatedAt,
                OrderItems = o.OrderItems.Select(oi => new OrderItemDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    UnitPrice = oi.UnitPrice,
                    Quantity = oi.Quantity
                }).ToList()
            }).ToList();

            return Ok(orderDtos);
        }

        // ✅ PUT: Cancel an order
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id, [FromQuery] string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("UserId is required.");

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
                return NotFound("Order not found.");

            if (order.Status != "Pending" && order.Status != "Processing")
                return BadRequest("Order cannot be canceled at this stage.");

            order.Status = "Cancelled";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Order canceled successfully." });
        }
    }
}
