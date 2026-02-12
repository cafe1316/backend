namespace Cafe1316.Application.DTOs;

public class CreateOrderDto
{
//CreateOrderDto - 负责"创建订单需要的所有信息"（包括两个地址 + 备注）
    public string Email { get; set; } = string.Empty;
    public OrderAddressDto ShippingAddress { get; set; }= null!;
    public OrderAddressDto? BillingAddress { get; set; }
    public string? Notes { get; set; }
}