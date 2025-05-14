using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Try_application.Database;
using Try_application.Database.Entities;

namespace Try_application.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BookmarksController : ControllerBase
    {
        private readonly AppDBContext _context;

        public BookmarksController(AppDBContext context)
        {
            _context = context;
        }

        // GET: /api/Bookmarks?userId=abc123
        [HttpGet]
        public async Task<IActionResult> GetBookmarks([FromQuery] string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("UserId is required.");

            var bookmarks = await _context.Bookmarks
                .Include(b => b.Book)
                .Where(b => b.UserId == userId)
                .ToListAsync();

            return Ok(bookmarks);
        }

        // DTO for POST
        public class BookmarkDto
        {
            public int BookId { get; set; }
        }

        // POST: /api/Bookmarks?userId=abc123
        [HttpPost]
        public async Task<IActionResult> AddBookmark([FromQuery] string userId, [FromBody] BookmarkDto dto)
        {
            if (dto == null || dto.BookId <= 0 || string.IsNullOrWhiteSpace(userId))
                return BadRequest("Invalid request data.");

            // Prevent duplicates
            bool exists = await _context.Bookmarks.AnyAsync(b => b.UserId == userId && b.BookId == dto.BookId);
            if (exists)
                return Conflict("Bookmark already exists.");

            var bookmark = new Bookmark
            {
                UserId = userId,
                BookId = dto.BookId,
                DateAdded = DateTime.UtcNow
            };

            _context.Bookmarks.Add(bookmark);
            await _context.SaveChangesAsync();

            return Ok(bookmark);
        }

        // DELETE: /api/Bookmarks/{bookId}?userId=abc123
        [HttpDelete("{bookId}")]
        public async Task<IActionResult> DeleteBookmark(int bookId, [FromQuery] string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("UserId is required.");

            var bookmark = await _context.Bookmarks
                .FirstOrDefaultAsync(b => b.UserId == userId && b.BookId == bookId);

            if (bookmark == null)
                return NotFound("Bookmark not found.");

            _context.Bookmarks.Remove(bookmark);
            await _context.SaveChangesAsync();

            return Ok("Bookmark removed.");
        }
    }
}