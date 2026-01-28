using Cafe1316.Domain.Common;
using Cafe1316.Domain.Enums;
namespace Cafe1316.Domain.Entities;

public class Account: BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; } 
    public LoginProvider Provider { get; set; }
    public string ProviderAccountId { get; set; } = string.Empty;
    public string? EmailAtLink { get; set; }

    //导航属性
    public User User { get; set; } = null!;
    
}