namespace Cafe1316.Application.DTOs;

public class OrderDto
{
    public int Id { get; set; }
    public Guid Uuid { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public OrderAddressDto ShippingAddress { get; set; } = null!;
    public OrderAddressDto? BillingAddress { get; set; }

    // 金额
    public decimal Subtotal { get; set; } 
    public decimal ShippingFee { get; set; } 
    public decimal Tax { get; set; } 
    public decimal GrandTotal { get; set; } 
    public string Currency { get; set; } = "AUD"; 

    public string Status { get; set; } = "Pending";
    public DateTime? PaidAt { get; set; }
    public string? Notes { get; set; }

    public List<OrderItemDto> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}