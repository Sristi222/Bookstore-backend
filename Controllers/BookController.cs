using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using Try_application.Database;
using Try_application.Database.Entities;
using Try_application.Model;

namespace Try_application.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDBContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductsController(AppDBContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        public IActionResult GetProducts(int page = 1, int limit = 10)
        {
            var totalItems = _context.Products.Count();
            var products = _context.Products
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToList();

            return Ok(new { total = totalItems, page, limit, data = products });
        }

        [HttpGet("search")]
        public IActionResult SearchProducts(string? q, string? sort = "name", int? minPrice = null, int? maxPrice = null)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(q))
                query = query.Where(p => p.Name.Contains(q) || p.Description.Contains(q));

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            query = sort switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "name" => query.OrderBy(p => p.Name),
                _ => query
            };

            return Ok(query.ToList());
        }

        // ✅ UPDATED: use ProductDto instead of Product
        [HttpPost]
        public async Task<IActionResult> AddProduct([FromForm] ProductDto dto, IFormFile image)
        {
            if (dto == null || image == null)
                return BadRequest(new { message = "Invalid product or image." });

            try
            {
                if (string.IsNullOrEmpty(_env.WebRootPath))
                    return StatusCode(500, new { message = "Web root path is not set." });

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                var uploadFolder = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadFolder);

                var filePath = Path.Combine(uploadFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                // ✅ map DTO → Product entity
                var product = new Product
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    Price = dto.Price,
                    Image = $"/uploads/{fileName}"
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error saving product", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(int id)
        {
            var prod = _context.Products.Find(id);
            if (prod == null)
                return NotFound();

            _context.Products.Remove(prod);
            _context.SaveChanges();
            return Ok();
        }
    }
}
