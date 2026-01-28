using Cafe1316.Domain.Common;
namespace Cafe1316.Domain.Entities;

public class Category : BaseEntity 
{
    public int Id { get; set; }=0;
    public string Slug { get; set; }= string.Empty;
    public string Name { get; set; }= string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }= 0;
    public bool IsActive { get; set; }= true;

    //导航属性
    public ICollection<Subcategory> Subcategories { get; set; }= new List<Subcategory>();
    public ICollection<Product> Products { get; set; }= new List<Product>();
}