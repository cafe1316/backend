namespace Cafe1316.Application.DTOs;

public class CartDto
{
    public List<CartItemDto> Items { get; set; } = new();

    // 统计信息（后端计算）
    public int TotalItems { get; set; } = 0;
    public decimal TotalAmount { get; set; } = 0;
    public string Currency { get; set; } = "AUD";
    public bool HasUnavailableItems { get; set; }
}
