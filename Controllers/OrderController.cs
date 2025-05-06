using Microsoft.AspNetCore.Mvc;
using Try_application.Database.Entities;
using Try_application.Database;
using Microsoft.EntityFrameworkCore;
using Try_application.Model;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly AppDBContext _context;

    public OrdersController(AppDBContext context)
    {
        _context = context;
    }

    // 👉 Place an order
    [HttpPost("place")]
    public async Task<IActionResult> PlaceOrder([FromBody] OrderDto dto)
    {
        var order = new Order
        {
            UserId = dto.UserId,
            Items = dto.Items.Select(i => new OrderItem { ProductId = i.ProductId, Quantity = i.Quantity }).ToList()
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return Ok(order);
    }

    // 👉 Cancel an order
    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> CancelOrder(int id, [FromQuery] string userId)
    {
        var order = _context.Orders.Include(o => o.Items).FirstOrDefault(o => o.Id == id && o.UserId == userId);

        if (order == null) return NotFound(new { message = "Order not found." });

        if (order.Status != "Pending")
            return BadRequest(new { message = "Order cannot be cancelled (already processed)." });

        order.Status = "Cancelled";
        await _context.SaveChangesAsync();

        return Ok(new { message = "Order cancelled." });
    }

    // 👉 Get orders for a user
    [HttpGet("user/{userId}")]
    public IActionResult GetUserOrders(string userId)
    {
        var orders = _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Where(o => o.UserId == userId)
            .ToList();

        return Ok(orders);
    }

}
