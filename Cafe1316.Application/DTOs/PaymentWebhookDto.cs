namespace Cafe1316.Application.DTOs;

public class PaymentWebhookDto
{
    // 事件类型，如 "payment_intent.succeeded"
    public string EventType { get; set; } = string.Empty;
    
    // 我们最关心的：Stripe 里的 PaymentIntentId (用于存库)
    public string StripePaymentIntentId { get; set; } = string.Empty;
    
    // 我们夹带的私货：用于找回 CheckoutIntent
    public Dictionary<string, string> Metadata { get; set; } = new();
}