using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Stripe.Checkout;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly DataContext _context;

    public OrdersController(DataContext context)
    {
        _context = context;
    }

    // GET: api/orders/session/{sessionId}
    [HttpGet("session/{sessionId}")]
    public async Task<ActionResult<Order>> GetOrderBySessionId(string sessionId)
    {
        var sessionService = new SessionService();
        Session stripeSession;

        try
        {
            stripeSession = await sessionService.GetAsync(sessionId);
        }
        catch (Stripe.StripeException ex)
        {
            return BadRequest($"Invalid session ID: {ex.Message}");
        }

        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.StripeSessionId == sessionId);

        if (order == null)
            return NotFound("Order not found");

        if (stripeSession.PaymentStatus == "paid" && order.Status != OrderStatus.Completed)
        {
            order.Status = OrderStatus.Completed;
            order.CompletedDate = DateTime.Now;
            order.StripePaymentIntentId = stripeSession.PaymentIntentId;
            await _context.SaveChangesAsync();
        }
        else if (stripeSession.PaymentStatus == "unpaid" && order.Status == OrderStatus.Pending)
        {
            order.Status = OrderStatus.Failed;
            await _context.SaveChangesAsync();
        }

        return Ok(order);
    }
}

