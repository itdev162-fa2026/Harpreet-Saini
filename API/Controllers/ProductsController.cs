using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Domain;
using Persistence;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly DataContext _context;

        public ProductsController(DataContext context)
        {
            _context = context;
        }

        // GET: /products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var products = await _context.Products.ToListAsync();
            return Ok(products);
        }

        // GET: /products/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProductById(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        // POST: /products
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage);
                return UnprocessableEntity(new { Errors = errors });
            }

            product.CreatedDate = DateTime.Now;
            product.LastUpdatedDate = DateTime.Now;

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
        }

        // PUT: /products/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<Product>> UpdateProduct(int id, Product updatedProduct)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage);
                return UnprocessableEntity(new { Errors = errors });
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            product.Name = updatedProduct.Name;
            product.Description = updatedProduct.Description;
            product.Price = updatedProduct.Price;
            product.IsOnSale = updatedProduct.IsOnSale;
            product.SalePrice = updatedProduct.SalePrice;
            product.CurrentStock = updatedProduct.CurrentStock;
            product.ImageUrl = updatedProduct.ImageUrl;
            product.LastUpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok(product);
        }

        // DELETE: /products/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: /products/search
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Product>>> SearchProducts(
            [FromQuery] string? name = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] bool? isOnSale = null,
            [FromQuery] bool? inStock = null,
            [FromQuery] string sortBy = "name",
            [FromQuery] string sortOrder = "asc")
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(name))
                query = query.Where(p => p.Name.ToLower().Contains(name.ToLower()));

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            if (isOnSale.HasValue)
                query = query.Where(p => p.IsOnSale == isOnSale.Value);

            if (inStock.HasValue && inStock.Value)
                query = query.Where(p => p.CurrentStock > 0);

            // Apply sorting
            query = (sortBy.ToLower(), sortOrder.ToLower()) switch
            {
                ("price", "desc") => query.OrderByDescending(p => p.Price),
                ("price", _) => query.OrderBy(p => p.Price),
                ("created", "desc") => query.OrderByDescending(p => p.CreatedDate),
                ("created", _) => query.OrderBy(p => p.CreatedDate),
                ("stock", "desc") => query.OrderByDescending(p => p.CurrentStock),
                ("stock", _) => query.OrderBy(p => p.CurrentStock),
                (_, "desc") => query.OrderByDescending(p => p.Name),
                _ => query.OrderBy(p => p.Name)
            };

            var products = await query.ToListAsync();
            return Ok(products);
        }
    }
}
