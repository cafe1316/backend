using Cafe1316.Domain.Common;
namespace Cafe1316.Domain.Entities;

public class CartItem : BaseEntity
{
    public int Id { get; set; } = 0;
    public Guid UserId { get; set; } 
    //UserId 是外键，应该引用已存在的 User.Id
    //自动生成新 Guid 会创建一个不存在的用户引用
    //这会导致数据库外键约束错误
    public int ProductId { get; set; } = 0;
    public int Quantity { get; set; } = 1;
    public DateTime AddedAt { get; set; } = DateTime.UtcNow; //DateTime.UtcNow 是属性，不是方法，没有括号

    //导航属性
    public User User { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
