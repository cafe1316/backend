namespace Cafe1316.Application.DTOs;

public class CreatePaymentSessionDto
{
    public int OrderId {get; set;}
}

public class PaymentSessionResultDto
{
    public string SessionId { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;

    // ✅ 新增：返回我们系统内部的 PaymentId
    public int PaymentId { get; set; }
}