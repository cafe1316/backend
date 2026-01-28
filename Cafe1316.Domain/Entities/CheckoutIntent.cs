using Cafe1316.Domain.Common;
namespace Cafe1316.Domain.Entities;

public class CheckoutIntent : BaseEntity
{
    public int Id { get; set; } = 0;
    public Guid Uuid { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; } 

    // JSON 字段（存储为字符串）
    public string SelectedItems { get; set; } = string.Empty;
    public string? ShippingAddress { get; set; }
    public string? BillingAddress { get; set; }
    public string? ShippingMethod { get; set; }

    // 金额
    public int SubtotalCents { get; set; } = 0;
    public int ShippingFeeCents { get; set; } = 0;
    public int TaxCents { get; set; } = 0;
    public int GrandTotalCents { get; set; } = 0;
    public string Currency { get; set; } = "AUD";

    // 状态
    public int? CompletedOrderId { get; set; }

    // 过期管理
    public DateTime ExpiresAt { get; set; }

    //导航属性
    public User User { get; set; } = null!;
    public Order? CompletedOrder { get; set; } // 可空属性不需要初始化
    //Order? 已经表示可以为 null
    //null! 是告诉编译器"这个值不会是 null"，矛盾了
    //可空导航属性不需要任何初始化
}