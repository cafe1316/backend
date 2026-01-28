using Cafe1316.Domain.Common;
using Cafe1316.Domain.Enums;
namespace Cafe1316.Domain.Entities;

public class Product : BaseEntity
{
    public int Id { get; set; }=0;
    public Guid Uuid { get; set; } = Guid.NewGuid();
    public string Sku { get; set; }= string.Empty;
    public int CategoryId { get; set; }=0;
    public int? SubcategoryId { get; set; }
    public string Name { get; set; }= string.Empty;
    public string Slug { get; set; }= string.Empty;
    public string? Description { get; set; }
    public int PriceCents { get; set; }=0;
    public string Currency { get; set; } = "AUD";
    public int Stock { get; set; } = 0;
    public string Unit { get; set; } = "bag";
    public string? Brand { get; set; }
    public int? Weight { get; set; }
    public CoffeeOrigin? Origin { get; set; }
    public RoastLevel? RoastLevel { get; set; }
    public ProcessingMethod? ProcessingMethod { get; set; }
    public int? Altitude { get; set; }
    public string? Varietals { get; set; }
    public int? HarvestYear { get; set; }
    public int? CuppingScore { get; set; }
    public string? Material { get; set; }
    public string? Color { get; set; }
    public string? Size { get; set; }
    public int? Capacity { get; set; }
    public string? Specifications { get; set; }
    public bool IsFeatured { get; set; } = false;
    public bool IsActive { get; set; } = true;
    
    //导航属性
    public Category Category { get; set; } = null!;
    public Subcategory? Subcategory { get; set; }
    public ICollection<ProductImage> Images { get; set; }= new List<ProductImage>();
    public ICollection<ProductFlavorNote> FlavorNotes { get; set; }= new List<ProductFlavorNote>();
    public ICollection<ProductTagMapping> Tags { get; set; }= new List<ProductTagMapping>();
    public ICollection<CartItem> CartItems{ get; set; }= new List<CartItem>();
    public ICollection<WishlistItem> WishlistItems{ get; set; }= new List<WishlistItem>();
    public ICollection<OrderItem> OrderItems{get; set; }= new List<OrderItem>();
}