using Cafe1316.Domain.Common;
namespace Cafe1316.Domain.Entities;

public class UserProfile : BaseEntity
{
    public int Id { get; set; }=0;
    public Guid UserId { get; set; } 
    public string? Nickname { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Gender { get; set; }
    public DateOnly? BirthDate { get; set; } //DateOnly 只存储日期（2024-01-27）
    public string? Phone { get; set; }
    public string? Bio { get; set; }

    //导航属性
    public User User { get; set; } = null!;

}