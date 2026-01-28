using Cafe1316.Domain.Common;
namespace Cafe1316.Domain.Entities;

public class WishlistItem : BaseEntity
{
    public int Id { get; set; } = 0;
    public Guid UserId { get; set; } 
    public int ProductId { get; set; } = 0;

    //导航属性
    public User User { get; set; } = null!;
    public Product Product { get; set; } = null!;

}