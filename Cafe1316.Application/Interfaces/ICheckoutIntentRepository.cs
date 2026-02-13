using Cafe1316.Domain.Entities;

namespace Cafe1316.Application.Interfaces;

public interface ICheckoutIntentRepository
{
    ///<summary>
    /// 作用: Phase 1 创建结账意图时用。
    ///返回值: 返回插入后的实体（会带上数据库生成的 Id）。
    ///场景: 用户点"结账"→ OrderService 调用这个方法把快照存进 checkout_intents 表。
    ///<summary>
    Task<CheckoutIntent> AddAsync(CheckoutIntent checkoutIntent, CancellationToken cancellationToken = default);
    
    ///<summary>
    /// 作用: Phase 2 Webhook 回调时用。
    ///参数: uuid 是我们贴在 Stripe Metadata 里的便利贴。
    ///返回值: ? 表示可为空。如果找不到（比如 UUID 错了），返回 null。
    ///场景: Stripe 通知我们"支付成功"→ 我们根据 Metadata 里的 UUID 找回之前的快照。
    ///<summary>   
    Task<CheckoutIntent?> GetByUuidAsync(Guid uuid, CancellationToken cancellationToken = default);

    ///<summary>
    ///作用: 支付成功后，把 checkoutIntent.CompletedOrderId 字段填上。
    ///场景: 订单生成后，把这个 Intent 标记为"已完成"，防止重复处理。
    ///<summary>
    Task UpdateAsync(CheckoutIntent checkoutIntent, CancellationToken cancellationToken = default);
}