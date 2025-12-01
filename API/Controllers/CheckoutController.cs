using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Stripe;
using Stripe.Checkout;
using System.ComponentModel.DataAnnotations;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckoutController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;

        public CheckoutController(DataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
        }

        [HttpPost("create-session")]
        public async Task<ActionResult<CreateSessionResponse>> CreateCheckoutSession(
            [FromBody] CreateCheckoutSessionRequest request)
        {
            // Validate request
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage);
                return UnprocessableEntity(new { Errors = errors });
            }

            if (request.Items.Count == 0)
                return BadRequest(new { Error = "Cart is empty" });

            // Fetch products from DB
            var productIds = request.Items.Select(i => i.ProductId).ToList();
            var products = await _context.Products
                                         .Where(p => productIds.Contains(p.Id))
                                         .ToListAsync();

            if (products.Count != request.Items.Count)
            {
                var missingIds = request.Items.Select(i => i.ProductId)
                                              .Except(products.Select(p => p.Id));
                return BadRequest(new { Error = $"Products not found: {string.Join(",", missingIds)}" });
            }

            // Prepare Stripe line items
            var lineItems = request.Items.Select(i =>
            {
                var product = products.First(p => p.Id == i.ProductId);

                return new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(product.Price * 100), // <-- FIXED: added comma
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = product.Name,
                            Description = product.Description,
                            Images = !string.IsNullOrEmpty(product.ImageUrl)
                                ? new List<string> { product.ImageUrl }
                                : new List<string>() // never null
                        }
                    },
                    Quantity = i.Quantity
                };
            }).ToList();

            // Create Stripe session
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = lineItems,
                Mode = "payment",
                CustomerEmail = request.CustomerEmail,
                SuccessUrl = _configuration["Stripe:SuccessUrl"] + "?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = _configuration["Stripe:CancelUrl"]
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            // Return session info
            var response = new CreateSessionResponse
            {
                SessionId = session.Id,
                Url = session.Url ?? string.Empty
            };

            return Ok(response);
        }
    }

    // Request and Response Models
    public class CreateCheckoutSessionRequest
    {
        [Required, EmailAddress]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required]
        public List<CartItemRequest> Items { get; set; } = new();
    }

    public class CreateSessionResponse
    {
        public string SessionId { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    public class CartItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
