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
    public string? SubcategoryName { get; set; }

    public string Unit { get; set; } = "bag";
    public string? Brand { get; set; }
    public int? Weight { get; set; }
    public string? Origin { get; set; }
    public string? RoastLevel { get; set; }
    public string? ProcessingMethod { get; set; }
    public int? Altitude { get; set; }
    public string? Varietals { get; set; }
    public int? HarvestYear { get; set; }
    public int? CuppingScore { get; set; }
    public string? Material { get; set; }
    public string? Color { get; set; }
    public string? Size { get; set; }
    public int? Capacity { get; set; }
    public string? Specifications { get; set; }
    public List<string> FlavorNotes { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public List<string> ImageUrls { get; set; }= new();
    public bool IsFeatured { get; set; } = false;

}