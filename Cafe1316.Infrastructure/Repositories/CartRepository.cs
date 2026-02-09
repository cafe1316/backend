using Cafe1316.Application.Interfaces;
using Cafe1316.Domain.Entities;
using Cafe1316.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cafe1316.Infrastructure.Repositories;

public class CartRepository : ICartRepository
{
    readonly ApplicationDbContext _context;

    public CartRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CartItem?> GetByUserAndProductAsync(Guid userId, int productId, CancellationToken cancellationToken = default)
    {
        return await _context.CartItems
            .Include(ci => ci.Product)
                .Include(ci => ci.Product.Images)
            .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productId, cancellationToken);
    }

    public async Task<CartItem?> GetByIdAsync(int cartItemId, CancellationToken cancellationToken = default)
    {
        return await _context.CartItems
            .Include(ci => ci.Product)
                .Include(ci => ci.Product.Images)
            .FirstOrDefaultAsync(ci => ci.Id == cartItemId, cancellationToken);
    }

    public async Task<List<CartItem>> GetUserCartItemsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.CartItems
            .Include(ci => ci.Product)
                .Include(ci => ci.Product.Images)
            .Where(ci => ci.UserId == userId)
            .OrderByDescending(ci => ci.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<CartItem> AddAsync(CartItem cartItem, CancellationToken cancellationToken = default)
    {
        await _context.CartItems.AddAsync(cartItem, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return cartItem;
    }

    public async Task UpdateAsync(CartItem cartItem, CancellationToken cancellationToken = default)
    {
        _context.CartItems.Update(cartItem);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(CartItem cartItem, CancellationToken cancellationToken = default)
    {
        _context.CartItems.Remove(cartItem); // ← 不查询，直接删除
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ClearUserCartAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var items = await _context.CartItems
            .Where(ci => ci.UserId == userId)
            .ToListAsync(cancellationToken);
        _context.CartItems.RemoveRange(items);
        await _context.SaveChangesAsync(cancellationToken);
    }
}