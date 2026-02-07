namespace Cafe1316.Application.DTOs;

public class ProductListDto
{
    // ===== 基本信息 =====
    public int Id {get; set;} = 0;
    public string Name {get; set;} = string.Empty;
    public string Slug { get; set; }= string.Empty;

    // ===== 价格信息 =====
    public decimal Price { get; set; } = 0;
    public string Currency { get; set; } = "AUD";
    public string Unit { get; set; } = "bag";

    // 库存状态（不暴露精确数量）
    public string StockStatus { get; set; } = "InStock"; // "InStock", "LowStock", "OutOfStock"

    // ===== 分类信息 =====
    public string CategoryName { get; set; } = string.Empty;
    public string? SubcategoryName { get; set; }

    // ===== 产品属性 =====
    public string? Brand { get; set; }
    public int? Weight { get; set; }
    public string? Origin { get; set; } 
    public string? RoastLevel { get; set; }

    // ===== 图片 =====
    public string? MainImageUrl { get; set; }  // 只需要主图

    // ===== 风味标签 =====
    public List<string> FlavorNotes { get; set; }= new();

    // ===== 标签 =====
    public List<string> Tags { get; set; } = new();

    // 特殊标记
    public bool IsFeatured { get; set; }
}