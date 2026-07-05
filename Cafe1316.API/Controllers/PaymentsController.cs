using Cafe1316.Application.DTOs;
using Cafe1316.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Cafe1316.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IOrderService orderService,
        IPaymentService paymentService,
        ILogger<PaymentsController> logger)
    {
        _orderService = orderService;
        _paymentService = paymentService;
        _logger = logger;
    }

    /// <summary>
    /// Phase 1: 创建结账意图（获取 Stripe ClientSecret）
    /// </summary>
    [HttpPost("checkout")]
    [Authorize]
    public async Task<ActionResult<CheckoutResponseDto>> CreateCheckout(
        [FromBody] CreateCheckoutIntentDto dto,
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized("Invalid token");
        var result = await _orderService.CreateCheckoutIntentAsync(userId, dto, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Phase 2: Stripe Webhook 回调（支付成功通知）
    /// </summary>
    [HttpPost("webhook")]
    public async Task<IActionResult> StripeWebhook(CancellationToken cancellationToken)
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"].ToString();
        
        if (string.IsNullOrEmpty(signature))
        {
            return BadRequest("Missing signature");
        }

        PaymentWebhookDto webhookData;
        try
        {
            webhookData = await _paymentService.ConstructEventAsync(json, signature);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Stripe webhook signature verification failed.");
            return BadRequest("Invalid webhook signature.");
        }

        try
        {
            if (webhookData.EventType == "payment_intent.succeeded")
            {
                var checkoutUuid = webhookData.Metadata.GetValueOrDefault("checkout_intent_uuid");
                
                if (string.IsNullOrEmpty(checkoutUuid))
                {
                    return BadRequest("Missing checkout_intent_uuid in metadata");
                }

                await _orderService.ProcessPaymentSuccessAsync(
                    checkoutUuid, 
                    webhookData.StripePaymentIntentId, 
                    cancellationToken);
            }

            return Ok();
        }
        catch (Exception ex)
        {
            // A 5xx tells Stripe that a valid event was not fully processed and should be retried.
            _logger.LogError(ex, "Failed to process Stripe event {EventType}.", webhookData.EventType);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
