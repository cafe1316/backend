namespace Cafe1316.Application.DTOs;

/// <summary>
/// 购物车单项
/// </summary>
public class CartItemDto
{
    // 购物车项ID
    public int Id { get; set; } = 0;

    // 商品信息
    public int ProductId { get; set; } = 0;
    public string ProductName { get; set; } = string.Empty;
    public string ProductSlug { get; set; } = string.Empty;
    public string? MainImageUrl { get; set; }

    // 价格
    public decimal Price { get; set; } = 0;
    public string Currency { get; set; } = "AUD";

    // 数量和库存
    public int Quantity { get; set; } = 1;
    public string StockStatus { get; set; } = "InStock";

    // 小计（后端计算）
    public decimal Subtotal { get; set; } = 0;
    
    public DateTime AddedAt { get; set; }

}