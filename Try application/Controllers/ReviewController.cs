using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Try_application.Database;
using Microsoft.EntityFrameworkCore;
using Try_application.Database.Entities;


[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReviewController : ControllerBase
{
    private readonly AppDBContext _context;

    public ReviewController(AppDBContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> AddReview([FromQuery] string userId, [FromQuery] int productId, [FromBody] Review review)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest("UserId required.");

        // ✅ check if user has completed order for this product
        bool hasCompletedOrder = await _context.Orders
            .Include(o => o.OrderItems)
            .AnyAsync(o => o.UserId == userId && o.Status == "Completed" && o.OrderItems.Any(oi => oi.ProductId == productId));

        if (!hasCompletedOrder)
            return BadRequest("You can only review a book you purchased and fulfilled.");

        // ✅ save review
        review.UserId = userId;
        review.ProductId = productId;
        review.CreatedAt = DateTime.UtcNow;

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Review submitted!" });
    }
}
