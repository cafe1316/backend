namespace Cafe1316.Application.DTOs;

public class ProductDetailDto
{
    // ===== 基本信息 =====
    public int Id { get; set; }=0;
    public string Name { get; set; }= string.Empty;
    public string Slug { get; set; }= string.Empty;
    public string? Description { get; set; }

    // ===== 价格和库存 =====
    public int Price { get; set; } = 0;
    public string Currency { get; set; } = "AUD";
    public string StockStatus { get; set; } = "InStock"; // "InStock", "LowStock", "OutOfStock"

    // ===== 分类 =====
    public string CategoryName { get; set; }=string.Empty;
    public string? SubcategoryId { get; set; }
}