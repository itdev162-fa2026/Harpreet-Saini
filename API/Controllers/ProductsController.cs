using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
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

        // GET all products
        [HttpGet]
        public ActionResult<IEnumerable<Product>> GetProducts()
        {
            return Ok(_context.Products.ToList());
        }

        // GET product by id
        [HttpGet("{id}")]
        public ActionResult<Product> GetProductById(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
                return NotFound();

            return Ok(product);
        }

        // POST: Create new product with validation and detailed error reporting
        [HttpPost]
        public ActionResult<Product> CreateProduct(Product product)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage);
                return UnprocessableEntity(new { Errors = errors });
            }

            product.CreatedDate = System.DateTime.Now;
            product.LastUpdatedDate = System.DateTime.Now;

            _context.Products.Add(product);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
        }

        // PUT: Update product with validation and detailed error reporting
        [HttpPut("{id}")]
        public ActionResult<Product> UpdateProduct(int id, Product product)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage);
                return UnprocessableEntity(new { Errors = errors });
            }

            var existingProduct = _context.Products.Find(id);

            if (existingProduct == null)
            {
                return NotFound();
            }

            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;
            existingProduct.IsOnSale = product.IsOnSale;
            existingProduct.SalePrice = product.SalePrice;
            existingProduct.CurrentStock = product.CurrentStock;
            existingProduct.ImageUrl = product.ImageUrl;
            existingProduct.LastUpdatedDate = System.DateTime.Now;

            _context.SaveChanges();

            return Ok(existingProduct);
        }

        // DELETE product by id
        [HttpDelete("{id}")]
        public ActionResult DeleteProduct(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
                return NotFound();

            _context.Products.Remove(product);
            _context.SaveChanges();

            return NoContent();
        }

        // SEARCH endpoint with filtering and sorting
        [HttpGet("search")]
        public ActionResult<IEnumerable<Product>> SearchProducts(
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
            {
                query = query.Where(p => p.Name.ToLower().Contains(name.ToLower()));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            if (isOnSale.HasValue)
            {
                query = query.Where(p => p.IsOnSale == isOnSale.Value);
            }

            if (inStock.HasValue && inStock.Value)
            {
                query = query.Where(p => p.CurrentStock > 0);
            }

            var products = query.ToList();

            products = sortBy.ToLower() switch
            {
                "price" => sortOrder.ToLower() == "desc"
                    ? products.OrderByDescending(p => p.Price).ToList()
                    : products.OrderBy(p => p.Price).ToList(),

                "created" => sortOrder.ToLower() == "desc"
                    ? products.OrderByDescending(p => p.CreatedDate).ToList()
                    : products.OrderBy(p => p.CreatedDate).ToList(),

                "stock" => sortOrder.ToLower() == "desc"
                    ? products.OrderByDescending(p => p.CurrentStock).ToList()
                    : products.OrderBy(p => p.CurrentStock).ToList(),

                _ => sortOrder.ToLower() == "desc"
                    ? products.OrderByDescending(p => p.Name).ToList()
                    : products.OrderBy(p => p.Name).ToList()
            };

            return Ok(products);
        }
    }
}
