using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Try_application.Database;
using Try_application.Database.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Try_application.Controllers
{
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

        /// <summary>
        /// Add a new review (allowed only if user completed order for this product)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        public async Task<IActionResult> AddReview(
            [FromQuery] string userId,
            [FromQuery] int productId,
            [FromBody] ReviewDto dto)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest(new { message = "UserId is required." });

            if (productId <= 0)
                return BadRequest(new { message = "ProductId is required." });

            if (dto == null)
                return BadRequest(new { message = "Review data is required." });

            if (dto.Rating < 1 || dto.Rating > 5)
                return BadRequest(new { message = "Rating must be between 1 and 5." });

            // ✅ check if user has completed an order for this product
            bool hasCompletedOrder = await _context.Orders
                .Include(o => o.OrderItems)
                .AnyAsync(o => o.UserId == userId &&
                               o.Status == "Completed" &&
                               o.OrderItems.Any(oi => oi.ProductId == productId));

            if (!hasCompletedOrder)
                return BadRequest(new { message = "You can only review a product you purchased and completed." });

            // ✅ check if already reviewed
            bool alreadyReviewed = await _context.Reviews
                .AnyAsync(r => r.UserId == userId && r.ProductId == productId);

            if (alreadyReviewed)
                return BadRequest(new { message = "You have already submitted a review for this product." });

            // ✅ save review
            var review = new Review
            {
                UserId = userId,
                ProductId = productId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return Ok(new { message = "✅ Review submitted successfully!" });
        }

        /// <summary>
        /// Get all reviews for a product (safe route to avoid conflict)
        /// </summary>
        [HttpGet("product/{productId}")] // ✅ updated route
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> GetReviewsForProduct([FromRoute] int productId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.ProductId == productId)
                .Select(r => new
                {
                    r.Id,
                    r.UserId,
                    r.Rating,
                    r.Comment,
                    r.CreatedAt,
                    ProductName = r.Product.Name,
                    ProductImage = r.Product.Image
                })
                .ToListAsync();

            return Ok(reviews);
        }
    }

    // ✅ DTO class
    public class ReviewDto
    {
        public int Rating { get; set; }
        public string Comment { get; set; }
    }
}
