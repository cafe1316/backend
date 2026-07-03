namespace Cafe1316.Application.DTOs;

public class CheckoutResponseDto
{
    //作用: 它包含了本次支付的所有敏感信息（金额、货币等）的加密签名。前端把它传给 Stripe.js，Stripe 就能知道用户要付多少钱，且无法篡改。
    public string ClientSecret { get; set; } = string.Empty;
    //Stripe 的公钥（以 pk_test_ 开头）
    public string PublishableKey { get; set; } = string.Empty;
    //作用: 状态关联。
    //当用户在前端支付成功后，Stripe 只会告诉我们 "PaymentIntent pi_123 成功了"。
    //我们需要知道这个 pi_123 对应的是我们数据库里的哪笔交易。
    //这个 ID 会在前端支付时作为 metadata 传给 Stripe，最后 Stripe Webhook 回调时再把这个 ID 带回来给我们。这就完成了一个闭环！
    public string CheckoutIntentId { get; set; } = string.Empty;
    public int SubtotalCents { get; set; }
    public int ShippingFeeCents { get; set; }
    public int TaxCents { get; set; }
    public int GrandTotalCents { get; set; }
    public string Currency { get; set; } = string.Empty;
}
