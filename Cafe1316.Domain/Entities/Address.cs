using Cafe1316.Domain.Common;
using Cafe1316.Domain.Enums;
namespace Cafe1316.Domain.Entities;

public class Address : BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; } 
    public AddressType Type { get; set; }
    public bool IsDefault { get; set; } = false;
    public string RecipientName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string AddressText { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public string CountryCode { get; set; } = "AU";

    //导航属性
    public User User { get; set; } = null!;
}
