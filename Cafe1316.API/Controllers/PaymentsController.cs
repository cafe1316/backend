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

    public PaymentsController(IOrderService orderService, IPaymentService paymentService)
    {
        _orderService = orderService;
        _paymentService = paymentService;
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
        Console.WriteLine("--> Stripe Webhook Hit!"); // 1. 证明请求到了
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"].ToString();
        
        if (string.IsNullOrEmpty(signature))
        {
            Console.WriteLine("--> Error: Missing Stripe-Signature header");
            return BadRequest("Missing signature");
        }

        try
        {
            // 1. 验证并解析 Webhook
            Console.WriteLine("--> Verifying Webhook Signature...");
            var webhookData = await _paymentService.ConstructEventAsync(json, signature);
            Console.WriteLine($"--> Webhook Verified. Event Type: {webhookData.EventType}");

            // 2. 只处理支付成功事件
            if (webhookData.EventType == "payment_intent.succeeded")
            {
                Console.WriteLine("--> Processing Payment Success...");
                var checkoutUuid = webhookData.Metadata.GetValueOrDefault("checkout_intent_uuid");
                
                if (string.IsNullOrEmpty(checkoutUuid))
                {
                    Console.WriteLine("--> Error: Missing checkout_intent_uuid meta");
                    return BadRequest("Missing checkout_intent_uuid in metadata");
                }

                Console.WriteLine($"--> Creating Order for Checkout UUID: {checkoutUuid}");
                // 3. 生成订单
                await _orderService.ProcessPaymentSuccessAsync(
                    checkoutUuid, 
                    webhookData.StripePaymentIntentId, 
                    cancellationToken);
                
                Console.WriteLine("--> Order Created Successfully!");
            }
            else 
            {
                Console.WriteLine($"--> Ignoring event type: {webhookData.EventType}");
            }

            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--> Webhook EXCEPTION: {ex.Message}");
            if (ex.InnerException != null) Console.WriteLine($"--> Inner: {ex.InnerException.Message}");
            return BadRequest($"Webhook error: {ex.Message}");
        }
    }
}