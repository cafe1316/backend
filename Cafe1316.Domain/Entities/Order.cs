using Cafe1316.Domain.Common;
using Cafe1316.Domain.Enums;
namespace Cafe1316.Domain.Entities;

public class Order : BaseEntity
{
    public int Id { get; set; } = 0;
    public Guid Uuid { get; set; } = Guid.NewGuid();
    public string OrderNumber { get; set; } = string.Empty;
    public Guid UserId { get; set; } 
    public string Email { get; set; } = string.Empty;

    // 地址快照（JSON）
    public string ShippingAddress { get; set; } = string.Empty;
    public string? BillingAddress { get; set; }

    // 金额
    public int SubtotalCents { get; set; } = 0;
    public int ShippingFeeCents { get; set; } = 0;
    public int TaxCents { get; set; } = 0;
    public int GrandTotalCents { get; set; } = 0;
    public string Currency { get; set; } = "AUD";   

    // 状态
    public OrderStatus Status { get; set; } = OrderStatus.Pending; //使用了 OrderStatus 枚举

    // 支付信息
    public string? StripeSessionId { get; set; }
    public string? StripePaymentIntentId { get; set; }
    public DateTime? PaidAt { get; set; }

    // 发货信息
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }

    // 备注
    public string? Notes { get; set; }

    //导航属性
    public User User { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; }= new List<OrderItem>();
    public Payment? Payment { get; set; }
    public ICollection<CheckoutIntent> CheckoutIntents { get; set; }= new List<CheckoutIntent>();

}
