using Cafe1316.Domain.Common;
namespace Cafe1316.Domain.Entities;

public class User : BaseEntity, ISoftDelete
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; }= string.Empty;
    public string? Password { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? DisplayName { get; set; }
    public DateTime? DeletedAt { get; set; }

    //导航属性
    public ICollection<Account> Accounts { get; set; }= new List<Account>();
    public UserProfile? Profile { get; set; }
    public ICollection<Address> Addresses { get; set; }= new List<Address>();
    public ICollection<CartItem> CartItems{ get; set; }= new List<CartItem>();
    public ICollection<WishlistItem> WishlistItems{ get; set; }= new List<WishlistItem>();
    public ICollection<Order> Orders{ get; set; }= new List<Order>();
    public ICollection<CheckoutIntent> CheckoutIntents{ get; set; }= new List<CheckoutIntent>();

}