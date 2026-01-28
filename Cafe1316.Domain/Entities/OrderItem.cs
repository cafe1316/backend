using Cafe1316.Domain.Common;
namespace Cafe1316.Domain.Entities;

public class OrderItem : BaseEntity
{
    public int Id { get; set; } = 0;
    public int OrderId { get; set; } = 0;
    public int ProductId { get; set; } = 0;

    //商品快照
    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public string? ProductSlug { get; set; } 
    public string? ImageUrl { get; set; } 

    //价格和数量
    public int UnitPriceCents { get; set; } = 0;
    public int Quantity { get; set; } = 0;
    public int LineTotalCents { get; set; } = 0;
    public string Currency { get; set; } = "AUD";

    //导航属性
    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
    
}