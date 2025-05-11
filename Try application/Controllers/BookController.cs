using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        // ✅ GET: /api/Products/trending
        [HttpGet("trending")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTrending()
        {
            var trendingBooks = await _context.Products
                .Where(p => p.IsTrending)
                .OrderByDescending(p => p.TotalSold)
                .ToListAsync();

            return Ok(trendingBooks);
        }

        // ✅ GET: /api/Products/bestsellers
        [HttpGet("bestsellers")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBestsellers()
        {
            var bestsellers = await _context.Products
                .Where(p => p.IsBestseller)
                .OrderByDescending(p => p.TotalSold)
                .ToListAsync();

            return Ok(bestsellers);
        }

        // ✅ GET: /api/Products/award-winners
        [HttpGet("award-winners")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAwardWinners()
        {
            var awards = await _context.Products
                .Where(p => p.HasAward)
                .ToListAsync();

            return Ok(awards);
        }

        // ✅ GET: /api/Products/new-releases
        [HttpGet("new-releases")]
        [AllowAnonymous]
        public async Task<IActionResult> GetNewReleases()
        {
            var newReleases = await _context.Products
                .Where(p => p.IsNewRelease)
                .OrderByDescending(p => p.PublicationDate)
                .ToListAsync();

            return Ok(newReleases);
        }

        // ✅ GET: /api/Products/new-arrivals
        [HttpGet("new-arrivals")]
        [AllowAnonymous]
        public async Task<IActionResult> GetNewArrivals()
        {
            var oneMonthAgo = DateTime.UtcNow.AddMonths(-1);
            var arrivals = await _context.Products
                .Where(p => p.DateAdded >= oneMonthAgo)
                .OrderByDescending(p => p.DateAdded)
                .ToListAsync();

            return Ok(arrivals);
        }

        // ✅ GET: /api/Products/coming-soon
        [HttpGet("coming-soon")]
        [AllowAnonymous]
        public async Task<IActionResult> GetComingSoon()
        {
            var comingSoon = await _context.Products
                .Where(p => p.IsComingSoon)
                .OrderBy(p => p.PublicationDate)
                .ToListAsync();

            return Ok(comingSoon);
        }

        // ✅ GET: /api/Products/deals
        [HttpGet("deals")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDeals()
        {
            var deals = await _context.Products
                .Where(p => p.IsOnDeal || (p.OnSale && p.DiscountPercent > 0))
                .ToListAsync();

            return Ok(deals);
        }

        // ✅ GET ALL products
        [HttpGet]
        public IActionResult GetProducts(int page = 1, int limit = 10)
        {
            var now = DateTime.UtcNow;
            var totalItems = _context.Products.Count();

            var products = _context.Products
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Image,
                    p.Author,
                    p.Genre,
                    p.Publisher,
                    p.ISBN,
                    p.Language,
                    p.Format,
                    p.PublicationDate,
                    p.Price,
                    p.DiscountPercent,
                    p.DiscountStartDate,
                    p.DiscountEndDate,
                    p.OnSale,
                    p.StockQuantity,
                    p.IsAvailableInStore,
                    p.HasAward,
                    p.IsTrending,
                    p.IsBestseller,
                    p.IsNewRelease,
                    p.IsComingSoon,
                    p.IsOnDeal,
                    p.DateAdded,
                    FinalPrice = (p.OnSale && p.DiscountPercent.HasValue &&
                                 (!p.DiscountStartDate.HasValue || p.DiscountStartDate <= now) &&
                                 (!p.DiscountEndDate.HasValue || p.DiscountEndDate >= now))
                                ? Math.Round(p.Price - (p.Price * (decimal)p.DiscountPercent.Value / 100), 2)
                                : p.Price
                })
                .ToList();

            return Ok(new { total = totalItems, page, limit, data = products });
        }

        // ✅ GET product by id
        [HttpGet("{id}")]
        public IActionResult GetProductById(int id)
        {
            var now = DateTime.UtcNow;

            var product = _context.Products
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Image,
                    p.Author,
                    p.Genre,
                    p.Publisher,
                    p.ISBN,
                    p.Language,
                    p.Format,
                    p.PublicationDate,
                    p.Price,
                    p.DiscountPercent,
                    p.DiscountStartDate,
                    p.DiscountEndDate,
                    p.OnSale,
                    p.StockQuantity,
                    p.IsAvailableInStore,
                    p.HasAward,
                    p.IsTrending,
                    p.IsBestseller,
                    p.IsNewRelease,
                    p.IsComingSoon,
                    p.IsOnDeal,
                    p.DateAdded,
                    FinalPrice = (p.OnSale && p.DiscountPercent.HasValue &&
                                 (!p.DiscountStartDate.HasValue || p.DiscountStartDate <= now) &&
                                 (!p.DiscountEndDate.HasValue || p.DiscountEndDate >= now))
                                ? Math.Round(p.Price - (p.Price * (decimal)p.DiscountPercent.Value / 100), 2)
                                : p.Price
                })
                .FirstOrDefault();

            if (product == null) return NotFound(new { message = "Product not found." });

            return Ok(product);
        }

        // ✅ SEARCH products
        [HttpGet("search")]
        public IActionResult SearchProducts(string? q, string? sort = "name", int? minPrice = null, int? maxPrice = null,
            bool? trending = null, bool? bestseller = null, bool? awardWinner = null, bool? newRelease = null, bool? comingSoon = null, bool? onDeal = null)
        {
            var now = DateTime.UtcNow;
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

            // Category filters
            if (trending.HasValue && trending.Value) query = query.Where(p => p.IsTrending);
            if (bestseller.HasValue && bestseller.Value) query = query.Where(p => p.IsBestseller);
            if (awardWinner.HasValue && awardWinner.Value) query = query.Where(p => p.HasAward);
            if (newRelease.HasValue && newRelease.Value) query = query.Where(p => p.IsNewRelease);
            if (comingSoon.HasValue && comingSoon.Value) query = query.Where(p => p.IsComingSoon);
            if (onDeal.HasValue && onDeal.Value) query = query.Where(p => p.IsOnDeal || (p.OnSale && p.DiscountPercent > 0));

            query = sort switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "name" => query.OrderBy(p => p.Name),
                "popularity" => query.OrderByDescending(p => p.TotalSold),
                "newest" => query.OrderByDescending(p => p.DateAdded),
                _ => query
            };

            var results = query.Select(p => new
            {
                p.Id,
                p.Name,
                p.Description,
                p.Image,
                p.Author,
                p.Genre,
                p.Publisher,
                p.ISBN,
                p.Language,
                p.Format,
                p.PublicationDate,
                p.Price,
                p.DiscountPercent,
                p.DiscountStartDate,
                p.DiscountEndDate,
                p.OnSale,
                p.StockQuantity,
                p.IsAvailableInStore,
                p.HasAward,
                p.IsTrending,
                p.IsBestseller,
                p.IsNewRelease,
                p.IsComingSoon,
                p.IsOnDeal,
                p.DateAdded,
                FinalPrice = (p.OnSale && p.DiscountPercent.HasValue &&
                             (!p.DiscountStartDate.HasValue || p.DiscountStartDate <= now) &&
                             (!p.DiscountEndDate.HasValue || p.DiscountEndDate >= now))
                            ? Math.Round(p.Price - (p.Price * (decimal)p.DiscountPercent.Value / 100), 2)
                            : p.Price
            }).ToList();

            return Ok(results);
        }

        // ✅ ADD PRODUCT - updated to return product with FinalPrice
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
                    Rating = 0,
                    DateAdded = DateTime.UtcNow,
                    HasAward = dto.HasAward,
                    IsTrending = dto.IsTrending,
                    IsBestseller = dto.IsBestseller,
                    IsNewRelease = dto.IsNewRelease,
                    IsComingSoon = dto.IsComingSoon,
                    IsOnDeal = dto.IsOnDeal
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                var now = DateTime.UtcNow;
                return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, new
                {
                    product.Id,
                    product.Name,
                    product.Description,
                    product.Image,
                    product.Author,
                    product.Genre,
                    product.Publisher,
                    product.ISBN,
                    product.Language,
                    product.Format,
                    product.PublicationDate,
                    product.Price,
                    product.DiscountPercent,
                    product.DiscountStartDate,
                    product.DiscountEndDate,
                    product.OnSale,
                    product.StockQuantity,
                    product.IsAvailableInStore,
                    product.HasAward,
                    product.IsTrending,
                    product.IsBestseller,
                    product.IsNewRelease,
                    product.IsComingSoon,
                    product.IsOnDeal,
                    product.DateAdded,
                    FinalPrice = (product.OnSale && product.DiscountPercent.HasValue &&
                                 (!product.DiscountStartDate.HasValue || product.DiscountStartDate <= now) &&
                                 (!product.DiscountEndDate.HasValue || product.DiscountEndDate >= now))
                                ? Math.Round(product.Price - (product.Price * (decimal)product.DiscountPercent.Value / 100), 2)
                                : product.Price
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error adding product.", error = ex.Message, inner = ex.InnerException?.Message });
            }
        }

        // ✅ UPDATE PRODUCT - updated to return product with FinalPrice
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

                // Category flags
                product.HasAward = dto.HasAward;
                product.IsTrending = dto.IsTrending;
                product.IsBestseller = dto.IsBestseller;
                product.IsNewRelease = dto.IsNewRelease;
                product.IsComingSoon = dto.IsComingSoon;
                product.IsOnDeal = dto.IsOnDeal;

                _context.Products.Update(product);
                await _context.SaveChangesAsync();

                var now = DateTime.UtcNow;
                var productDto = new
                {
                    product.Id,
                    product.Name,
                    product.Description,
                    product.Image,
                    product.Author,
                    product.Genre,
                    product.Publisher,
                    product.ISBN,
                    product.Language,
                    product.Format,
                    product.PublicationDate,
                    product.Price,
                    product.DiscountPercent,
                    product.DiscountStartDate,
                    product.DiscountEndDate,
                    product.OnSale,
                    product.StockQuantity,
                    product.IsAvailableInStore,
                    product.HasAward,
                    product.IsTrending,
                    product.IsBestseller,
                    product.IsNewRelease,
                    product.IsComingSoon,
                    product.IsOnDeal,
                    product.DateAdded,
                    FinalPrice = (product.OnSale && product.DiscountPercent.HasValue &&
                                 (!product.DiscountStartDate.HasValue || product.DiscountStartDate <= now) &&
                                 (!product.DiscountEndDate.HasValue || product.DiscountEndDate >= now))
                                ? Math.Round(product.Price - (product.Price * (decimal)product.DiscountPercent.Value / 100), 2)
                                : product.Price
                };

                return Ok(productDto);
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