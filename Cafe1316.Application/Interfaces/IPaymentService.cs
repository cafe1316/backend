using Cafe1316.Application.DTOs;

namespace Cafe1316.Application.Interfaces;

public interface IPaymentService
{
    // 1. 创建支付意图
    Task<string> CreatePaymentIntentAsync(int amountCents, string currency, Dictionary<string, string>? metadata = null, CancellationToken cancellationToken = default);
    // 2. 解析 Webhook 事件
    Task<PaymentWebhookDto> ConstructEventAsync(string json, string signature);
}