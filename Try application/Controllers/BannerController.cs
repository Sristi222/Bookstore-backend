using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Reflection;
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

        // ✅ GET active banner (checks if current time within StartDateTime - EndDateTime)
        [HttpGet]
        public IActionResult GetActiveBanner()
        {
            var now = DateTime.UtcNow;
            var banner = _context.Banners
                .Where(b =>
                    (!b.StartDateTime.HasValue || b.StartDateTime <= now) &&
                    (!b.EndDateTime.HasValue || b.EndDateTime >= now))
                .FirstOrDefault();

            if (banner == null)
                return NotFound(new { message = "No active banner at this time." });

            return Ok(banner);
        }

        // ✅ UPDATE banner (admin can edit title, subtitle, start/end datetime, and optionally upload image)
        [HttpPut]
        public async Task<IActionResult> UpdateBanner([FromForm] BannerDto dto, IFormFile? image)
        {
            var banner = _context.Banners.FirstOrDefault();
            if (banner == null)
            {
                banner = new Banner();
                _context.Banners.Add(banner);
            }

            banner.Title = dto.Title;
            banner.SubTitle = dto.SubTitle;
            banner.StartDateTime = dto.StartDateTime;
            banner.EndDateTime = dto.EndDateTime;
            banner.UpdatedAt = DateTime.UtcNow;

            if (image != null)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
                var uploadFolder = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadFolder);

                var filePath = Path.Combine(uploadFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                banner.ImageUrl = $"/uploads/{fileName}";
            }

            await _context.SaveChangesAsync();
            return Ok(banner);
        }
    }
}
