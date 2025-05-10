using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Try_application.Database; // Using Database instead of Data
using Try_application.Database.Entities; // Using Database.Entities instead of Models
using Try_application.Hubs;
using Try_application.Model; // Make sure this namespace exists for DTOs
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Net.Mail;

namespace Try_application.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly AppDBContext _context; // Using AppDBContext instead of ApplicationDbContext
        private readonly IHubContext<CustomerNotificationHub> _hubContext;

        public OrdersController(AppDBContext context, IHubContext<CustomerNotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
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

            var claimCode = GenerateClaimCode();

            var order = new Order
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow, // Using CreatedAt instead of OrderDate
                Status = "Pending",
                TotalAmount = cartItems.Sum(c => c.Product.Price * c.Quantity),
                ClaimCode = claimCode,
                OrderItems = cartItems.Select(c => new OrderItem
                {
                    ProductId = c.ProductId,
                    UnitPrice = c.Product.Price,
                    Quantity = c.Quantity
                }).ToList()
            };

            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            // ✅ Send email to user
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                var itemRows = string.Join("", order.OrderItems.Select(item => $@"
                    <tr>
                        <td>{item.Product.Name}</td>
                        <td>{item.Quantity}</td>
                        <td>Rs. {item.UnitPrice:F2}</td>
                        <td>Rs. {(item.Quantity * item.UnitPrice):F2}</td>
                    </tr>
                "));

                var emailBody = $@"
                    <html>
                    <body>
                        <h2>✅ Thanks for your order #{order.Id}!</h2>
                        <p>Hello {user.FullName},</p>
                        <p>Your claim code: <strong>{order.ClaimCode}</strong></p>
                        <h3>Order Bill:</h3>
                        <table border='1' cellpadding='5' cellspacing='0'>
                            <tr>
                                <th>Book</th>
                                <th>Qty</th>
                                <th>Unit Price</th>
                                <th>Subtotal</th>
                            </tr>
                            {itemRows}
                            <tr>
                                <td colspan='3' align='right'><strong>Total:</strong></td>
                                <td><strong>Rs. {order.TotalAmount:F2}</strong></td>
                            </tr>
                        </table>
                        <p>Please present this claim code at the counter to fulfill your order.</p>
                        <p>Regards,<br>Bookstore Team</p>
                    </body>
                    </html>
                ";

                await SendEmail(user.Email, "Your Claim Code & Bill", emailBody);
            }

            var dto = new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                CreatedAt = order.CreatedAt, // Using CreatedAt instead of OrderDate
                ClaimCode = order.ClaimCode,
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

        // Rest of the controller code remains the same...
        // ✅ GET: Orders for user
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
                CreatedAt = o.CreatedAt, // Using CreatedAt instead of OrderDate
                ClaimCode = o.ClaimCode,
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

        // ✅ GET: ALL orders (for staff)
        [HttpGet("all")]
        [Authorize(Roles = "Staff")]
        public async Task<ActionResult<List<OrderDto>>> GetAllOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ToListAsync();

            var orderDtos = orders.Select(o => new OrderDto
            {
                Id = o.Id,
                UserId = o.UserId,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                CreatedAt = o.CreatedAt, // Using CreatedAt instead of OrderDate
                ClaimCode = o.ClaimCode,
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

        // ✅ PUT: Cancel order
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

        // ✅ POST: Process Claim Code (staff action) - WITH SIGNALR NOTIFICATIONS
        [HttpPost("process-claim")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> ProcessClaim([FromQuery] string claimCode)
        {
            if (string.IsNullOrWhiteSpace(claimCode))
                return BadRequest("Claim code is required.");

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.ClaimCode == claimCode);

            if (order == null)
                return NotFound("Invalid claim code. Order not found.");

            if (order.Status == "Completed")
                return BadRequest("This order is already completed.");

            if (order.Status == "Cancelled")
                return BadRequest("This order was cancelled and cannot be fulfilled.");

            order.Status = "Completed";
            await _context.SaveChangesAsync();

            // Broadcast notification about the completed order
            int bookCount = order.OrderItems.Sum(oi => oi.Quantity);

            // Basic notification
            await _hubContext.Clients.All.SendAsync("ReceiveNotification",
                $"A customer just bought {bookCount} books!");

            return Ok(new { message = $"Order #{order.Id} marked as Completed." });
        }

        // ✅ Helper: Generate Claim Code
        private string GenerateClaimCode()
        {
            return Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
        }

        // ✅ Helper: Send Email
        private async Task SendEmail(string toEmail, string subject, string htmlBody)
        {
            using (var client = new SmtpClient("smtp.gmail.com"))
            {
                client.Port = 587;
                client.Credentials = new System.Net.NetworkCredential("sristishrestha80@gmail.com", "zyiy rlxx oypw wvjn");
                client.EnableSsl = true;

                var mail = new MailMessage("sristishrestha80@gmail.com", toEmail, subject, htmlBody);
                mail.IsBodyHtml = true;
                await client.SendMailAsync(mail);
            }
        }
    }
}