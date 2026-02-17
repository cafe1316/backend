using Cafe1316.Application.Interfaces;
using Cafe1316.Application.Settings;
using Cafe1316.Application.DTOs; // 别忘了引用 DTO
using Microsoft.Extensions.Options;
using Stripe; // Stripe SDK

namespace Cafe1316.Infrastructure.Services;

///<summary>
/// 它们需要配置信息（比如 API Key、SMTP服务器地址），所以依赖 Settings (配置)。它们通常不直接操作数据库（除非是为了记录日志），因为那是 Repository 的活。
///你可以把它想象成: “翻译官”。翻译官不需要知道公司有多少库存，他只需要一本字典（SDK）和对方的电话号码（Settings），负责把经理的话翻译给外面的人听。
///<summary>

public class StripePaymentService : IPaymentService
{
    private readonly StripeSettings _settings;
    // 1. 注入配置
    public StripePaymentService(IOptions<StripeSettings> settings)
    {
        _settings = settings.Value;
        
        // DEBUG LOGGING
        Console.WriteLine($"--> Stripe Service Initialized.");
        Console.WriteLine($"--> SecretKey Present: {!string.IsNullOrEmpty(_settings.SecretKey)}");
        Console.WriteLine($"--> WebhookSecret Present: {!string.IsNullOrEmpty(_settings.WebhookSecret)}");

        // 关键：全局配置 Stripe API Key
        StripeConfiguration.ApiKey = _settings.SecretKey;
    }

    // 2. 实现 CreatePaymentIntentAsync
    public async Task<string> CreatePaymentIntentAsync(
        int amountCents, 
        string currency, 
        Dictionary<string, string>? metadata = null, 
        CancellationToken cancellationToken = default)
    {
        var options = new PaymentIntentCreateOptions
        {
            Amount = amountCents,
            Currency = currency,
            // 开启自动支付方式（卡、钱包等）
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true,
            },
            Metadata = metadata // 贴便利贴
        };

        var service = new PaymentIntentService();
        var paymentIntent = await service.CreateAsync(options, cancellationToken: cancellationToken);
        return paymentIntent.ClientSecret; // 返回给前端的“支付令牌”
    }

    // 3. 实现 ConstructEventAsync
    public async Task<PaymentWebhookDto> ConstructEventAsync(string json, string signature)
    {
        await Task.CompletedTask; // Silence warning CS1998
        try
        {
            // 验证签名：确保是 Stripe 发来的
            // 验证签名：确保是 Stripe 发来的
            var stripeEvent = EventUtility.ConstructEvent(
                json, 
                signature, 
                _settings.WebhookSecret,
                throwOnApiVersionMismatch: false // 关键修复：忽略版本不匹配错误
            );

            // 1. 初始化我们要返回的 DTO
            var dto = new PaymentWebhookDto
            {
                EventType = stripeEvent.Type, // 比如 "payment_intent.succeeded"
            };
            // 2. 如果是“支付成功”事件，我们要提取里面的数据
            if (stripeEvent.Type == "payment_intent.succeeded")
            {
                // stripeEvent.Data.Object 是个通用的 object，我们要把它强转成 PaymentIntent
                if (stripeEvent.Data.Object is PaymentIntent intent)
                {
                    dto.StripePaymentIntentId = intent.Id; // Stripe 的 ID (pi_xxx)
                    dto.Metadata = intent.Metadata;        // 我们之前贴的便利贴
                }
            }
            return dto;
        }
        catch (StripeException e)
        {
            // 如果验钞失败（签名不对），抛出异常，Controller 会捕获并返回 400
            throw new Exception($"Webhook 签名验证失败: {e.Message}");
        }

    }
}