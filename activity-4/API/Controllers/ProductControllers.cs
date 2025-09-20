using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ILogger<ProductsController> _logger;
    private readonly DataContext _context;

    public ProductsController(ILogger<ProductsController> logger, DataContext context)
    {
        _logger = logger;
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    {
        var products = await _context.Products.ToListAsync();
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct(Product product)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        product.CreatedDate = DateTime.UtcNow;
        product.LastUpdatedDate = DateTime.UtcNow;

        await _context.Products.AddAsync(product);
        var success = await _context.SaveChangesAsync() > 0;

        if (success)
        {
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        _logger.LogError("Failed to create product");

        return BadRequest("Failed to create product");
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Product>> UpdateProduct(int id, Product product)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existingProduct = await _context.Products.FindAsync(id);

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

        existingProduct.LastUpdatedDate = DateTime.UtcNow;

        var success = await _context.SaveChangesAsync() > 0;

        if (success)
        {
            return Ok(existingProduct);
        }

        return BadRequest("Failed to update product");
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        _context.Products.Remove(product);
        var success = await _context.SaveChangesAsync() > 0;

        if (success)
        {
            return NoContent();
        }

        return BadRequest("Failed to delete product");
    }
}
