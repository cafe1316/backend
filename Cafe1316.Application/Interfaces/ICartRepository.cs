using Cafe1316.Domain.Entities;

namespace Cafe1316.Application.Interfaces;

public interface ICartRepository
{
    Task<CartItem?> GetByUserAndProductAsync(Guid userId, int productId, CancellationToken cancellationToken = default);//查询用户购物车里是否已经有这个商品
    Task<CartItem?> GetByIdAsync(int cartItemId, CancellationToken cancellationToken = default); //根据ID查询购物车项
    Task<List<CartItem>> GetUserCartItemsAsync(Guid userId, CancellationToken cancellationToken = default);//获取用户购物车里的所有商品
    Task<CartItem> AddAsync(CartItem cartItem, CancellationToken cancellationToken = default);//在购物车里创建一个新的商品项
    Task UpdateAsync(CartItem cartItem, CancellationToken cancellationToken = default);//更新购物车里的商品
    Task DeleteAsync(CartItem cartItem, CancellationToken cancellationToken = default);//删除
    Task ClearUserCartAsync(Guid userId, CancellationToken cancellationToken = default);//清空购物车
}