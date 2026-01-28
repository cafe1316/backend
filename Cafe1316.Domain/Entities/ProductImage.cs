using Cafe1316.Domain.Common;
namespace Cafe1316.Domain.Entities;

public class ProductImage : BaseEntity
{
    public int Id { get; set; }=0;
    public int ProductId { get; set; }=0;
    public string ImageUrl { get; set; }= string.Empty;
    public int DisplayOrder { get; set; } = 0;
    public bool IsPrimary { get; set; } = false;
   
   //导航属性
   public Product Product { get; set; } = null!;
}