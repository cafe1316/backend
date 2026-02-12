namespace Cafe1316.Application.DTOs;

public class OrderItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    //商品快照
    public string ProductName { get; set; } = string.Empty;
    public string? ProductSlug { get; set; } 
    public string? ImageUrl { get; set; }

    //价格和数量
    public decimal UnitPrice { get; set; } // Cents → Decimal
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; } // Cents → Decimal
    public string Currency { get; set; } = "AUD";
}