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
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"].ToString();
        if (string.IsNullOrEmpty(signature))
            return BadRequest("Missing signature");
        try
        {
            // 1. 验证并解析 Webhook
            var webhookData = await _paymentService.ConstructEventAsync(json, signature);
            // 2. 只处理支付成功事件
            if (webhookData.EventType == "payment_intent.succeeded")
            {
                var checkoutUuid = webhookData.Metadata.GetValueOrDefault("checkout_intent_uuid");
                if (string.IsNullOrEmpty(checkoutUuid))
                    return BadRequest("Missing checkout_intent_uuid in metadata");
                // 3. 生成订单
                await _orderService.ProcessPaymentSuccessAsync(
                    checkoutUuid, 
                    webhookData.StripePaymentIntentId, 
                    cancellationToken);
            }
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest($"Webhook error: {ex.Message}");
        }
    }
}