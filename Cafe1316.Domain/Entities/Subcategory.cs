using Cafe1316.Domain.Common;
namespace Cafe1316.Domain.Entities;

public class Subcategory : BaseEntity
{
    public int Id { get; set; }=0;
    public int CategoryId { get; set; }=0;
    public string Slug { get; set; }=string.Empty;
    public string Name { get; set; }=string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }=0;
    public bool IsActive { get; set; }=true;


    //导航属性
    public Category Category { get; set; } = null!;//（多对一，属于哪个分类）
    public ICollection<Product> Products { get; set; }= new List<Product>(); //（一对多）
}