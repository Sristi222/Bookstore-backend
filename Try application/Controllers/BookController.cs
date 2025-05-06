using Microsoft.AspNetCore.Mvc;
using System;
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
    public class ProductsController : ControllerBase
    {
        private readonly AppDBContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductsController(AppDBContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ✅ GET ALL with pagination
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

        // ✅ GET single product
        [HttpGet("{id}")]
        public IActionResult GetProductById(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound(new { message = "Product not found." });
            return Ok(product);
        }

        // ✅ SEARCH
        [HttpGet("search")]
        public IActionResult SearchProducts(string? q, string? sort = "name", int? minPrice = null, int? maxPrice = null)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(q))
            {
                query = query.Where(p =>
                    (p.Name != null && p.Name.Contains(q)) ||
                    (p.Description != null && p.Description.Contains(q)) ||
                    (p.Author != null && p.Author.Contains(q)) ||
                    (p.Genre != null && p.Genre.Contains(q)) ||
                    (p.ISBN != null && p.ISBN.Contains(q))
                );
            }

            if (minPrice.HasValue) query = query.Where(p => p.Price >= minPrice.Value);
            if (maxPrice.HasValue) query = query.Where(p => p.Price <= maxPrice.Value);

            query = sort switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "name" => query.OrderBy(p => p.Name),
                "popularity" => query.OrderByDescending(p => p.TotalSold),
                _ => query
            };

            return Ok(query.ToList());
        }

        // ✅ ADD PRODUCT
        [HttpPost]
        public async Task<IActionResult> AddProduct([FromForm] ProductDto dto, IFormFile image)
        {
            if (dto == null || image == null)
                return BadRequest(new { message = "Product or image is missing." });

            try
            {
                if (string.IsNullOrEmpty(_env.WebRootPath))
                    return StatusCode(500, new { message = "WebRootPath is not configured." });

                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                var product = new Product
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    Price = dto.Price,
                    Author = dto.Author,
                    Genre = dto.Genre,
                    Publisher = dto.Publisher,
                    ISBN = dto.ISBN,
                    Language = dto.Language,
                    Format = dto.Format,
                    PublicationDate = dto.PublicationDate?.ToUniversalTime(),
                    DiscountPercent = dto.DiscountPercent,
                    DiscountStartDate = dto.DiscountStartDate?.ToUniversalTime(),
                    DiscountEndDate = dto.DiscountEndDate?.ToUniversalTime(),
                    OnSale = dto.OnSale,
                    StockQuantity = dto.StockQuantity,
                    IsAvailableInStore = dto.IsAvailableInStore,
                    Image = $"/uploads/{fileName}",
                    TotalSold = 0,
                    Rating = 0
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error adding product.", error = ex.Message, inner = ex.InnerException?.Message });
            }
        }

        // ✅ UPDATE PRODUCT
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] ProductDto dto, IFormFile? image)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound(new { message = "Product not found." });

            try
            {
                if (image != null)
                {
                    var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                    Directory.CreateDirectory(uploadsFolder);

                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    product.Image = $"/uploads/{fileName}";
                }

                product.Name = dto.Name;
                product.Description = dto.Description;
                product.Price = dto.Price;
                product.Author = dto.Author;
                product.Genre = dto.Genre;
                product.Publisher = dto.Publisher;
                product.ISBN = dto.ISBN;
                product.Language = dto.Language;
                product.Format = dto.Format;
                product.PublicationDate = dto.PublicationDate?.ToUniversalTime();
                product.DiscountPercent = dto.DiscountPercent;
                product.DiscountStartDate = dto.DiscountStartDate?.ToUniversalTime();
                product.DiscountEndDate = dto.DiscountEndDate?.ToUniversalTime();
                product.OnSale = dto.OnSale;
                product.StockQuantity = dto.StockQuantity;
                product.IsAvailableInStore = dto.IsAvailableInStore;

                _context.Products.Update(product);
                await _context.SaveChangesAsync();

                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating product.", error = ex.Message, inner = ex.InnerException?.Message });
            }
        }

        // ✅ DELETE PRODUCT
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound(new { message = "Product not found." });

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Product deleted." });
        }
    }
}
