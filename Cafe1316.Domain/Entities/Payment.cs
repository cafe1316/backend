using Cafe1316.Domain.Common;
using Cafe1316.Domain.Enums;
namespace Cafe1316.Domain.Entities;

public class Payment : BaseEntity
{
    public int Id { get; set; } = 0;
    public Guid Uuid { get; set; } = Guid.NewGuid();

    public int OrderId { get; set; } =0;
    public PaymentMethod PaymentMethod { get; set; }
    public string? TransactionId { get; set; }
    public int AmountCents { get; set; } = 0;
    public string Currency { get; set; } = "AUD";
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTime? PaidAt { get; set; }

    //导航属性
    public Order Order { get; set; } = null!;
}