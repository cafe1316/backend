namespace Cafe1316.Domain.Enums;

public enum OrderStatus
{
    Pending,      // 待支付
    Paid,         // 已支付 (包含处理中)
    Shipped,      // 已发货 (包含配送中)
    Completed,    // 已完成
    Cancelled
}