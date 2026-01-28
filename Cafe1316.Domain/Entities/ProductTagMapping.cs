using Cafe1316.Domain.Common;
using Cafe1316.Domain.Enums;
namespace Cafe1316.Domain.Entities;

public class ProductTagMapping : BaseEntity
{
    public int Id { get; set; }=0;
    public int ProductId { get; set; }=0;
    public ProductTag Tag { get; set; }

    //导航属性
    public Product Product { get; set; } = null!;
}