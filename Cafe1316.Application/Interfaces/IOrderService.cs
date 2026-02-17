using Cafe1316.Application.DTOs;
using Cafe1316.Domain.Enums;

namespace Cafe1316.Application.Interfaces;

public interface IOrderService
{
    // ⚠️ DEPRECATED: 旧版方法已废弃，现在统一使用 Stripe 支付流程
    // Task<OrderDto> CreateOrderAsync(Guid userId, CreateOrderDto dto, CancellationToken cancellationToken = default);
    
    Task<OrderDto?> GetOrderByIdAsync(Guid userId, int orderId, CancellationToken cancellationToken = default);
    
    Task<OrderDto?> GetOrderByCheckoutIntentIdAsync(Guid userId, Guid checkoutIntentId, CancellationToken cancellationToken = default);

    Task<PaginatedResult<OrderDto>> GetUserOrdersAsync(
        Guid userId, int page, int pageSize, OrderStatus? status,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Phase 1: 创建结账意图并获取 Stripe ClientSecret
    /// 作用: 用户点击"结账"后调用，生成快照并向 Stripe 预登记
    /// 返回: CheckoutResponseDto (包含 ClientSecret 给前端)
    /// </summary>
    Task<CheckoutResponseDto> CreateCheckoutIntentAsync(
        Guid userId, 
        CreateCheckoutIntentDto dto, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Phase 2: Webhook 回调，将 CheckoutIntent 转化为正式 Order
    /// 作用: Stripe 支付成功后，通过 Webhook 触发此方法
    /// 参数: checkoutIntentUuid - 从 Stripe Metadata 里取回的 UUID
    /// 参数: stripePaymentIntentId - Stripe 的交易ID (pi_xxx)
    /// 返回: 新创建的 OrderDto
    /// </summary>
    Task<OrderDto> ProcessPaymentSuccessAsync(
        string checkoutIntentUuid, 
        string stripePaymentIntentId, 
        CancellationToken cancellationToken = default);
}
