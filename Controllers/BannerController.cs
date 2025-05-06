using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Try_application.Database;
using Try_application.Database.Entities;
using Try_application.Model;

namespace Try_application.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BannersController : ControllerBase
    {
        private readonly AppDBContext _context;
        private readonly IWebHostEnvironment _env;

        public BannersController(AppDBContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ✅ GET ALL banners
        [HttpGet]
        public IActionResult GetAllBanners()
        {
            var banners = _context.Banners.ToList();
            return Ok(banners);
        }

        // ✅ GET ACTIVE banner
        [HttpGet("active")]
        public IActionResult GetActiveBanner()
        {
            var banner = _context.Banners.FirstOrDefault(b => b.IsActive);
            if (banner == null)
                return NotFound(new { message = "No active banner." });

            return Ok(banner);
        }

        // ✅ CREATE new banner
        [HttpPost]
        public async Task<IActionResult> CreateBanner([FromForm] BannerDto dto, IFormFile? image)
        {
            var banner = new Banner
            {
                Title = dto.Title,
                SubTitle = dto.SubTitle,
                StartDateTime = dto.StartDateTime.HasValue ? DateTime.SpecifyKind(dto.StartDateTime.Value, DateTimeKind.Utc) : null,
                EndDateTime = dto.EndDateTime.HasValue ? DateTime.SpecifyKind(dto.EndDateTime.Value, DateTimeKind.Utc) : null,
                UpdatedAt = DateTime.UtcNow,
                IsActive = false // default inactive
            };

            if (image != null && image.Length > 0)
            {
                banner.ImageUrl = await SaveImageFile(image);
            }

            _context.Banners.Add(banner);
            await _context.SaveChangesAsync();

            return Ok(banner);
        }

        // ✅ UPDATE banner
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBanner(int id, [FromForm] BannerDto dto, IFormFile? image)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner == null) return NotFound(new { message = "Banner not found." });

            banner.Title = dto.Title;
            banner.SubTitle = dto.SubTitle;
            banner.StartDateTime = dto.StartDateTime.HasValue ? DateTime.SpecifyKind(dto.StartDateTime.Value, DateTimeKind.Utc) : null;
            banner.EndDateTime = dto.EndDateTime.HasValue ? DateTime.SpecifyKind(dto.EndDateTime.Value, DateTimeKind.Utc) : null;
            banner.UpdatedAt = DateTime.UtcNow;

            if (image != null && image.Length > 0)
            {
                banner.ImageUrl = await SaveImageFile(image);
            }

            await _context.SaveChangesAsync();
            return Ok(banner);
        }

        // ✅ DELETE banner
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBanner(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner == null) return NotFound(new { message = "Banner not found." });

            _context.Banners.Remove(banner);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Banner deleted." });
        }

        // ✅ ACTIVATE banner (only one active at a time)
        [HttpPut("{id}/activate")]
        public async Task<IActionResult> ActivateBanner(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner == null) return NotFound(new { message = "Banner not found." });

            foreach (var b in _context.Banners)
                b.IsActive = (b.Id == id);

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Banner {id} is now active." });
        }

        // ✅ helper: save image to /uploads
        private async Task<string> SaveImageFile(IFormFile image)
        {
            var uploadFolder = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadFolder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
            var filePath = Path.Combine(uploadFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await image.CopyToAsync(stream);

            return $"/uploads/{fileName}";
        }
    }
}
