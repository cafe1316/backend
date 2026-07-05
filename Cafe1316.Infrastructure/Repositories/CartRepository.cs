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

    public async Task<bool> TryAddQuantityAsync(
        Guid userId,
        int productId,
        int quantity,
        int maxQuantity,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var affectedRows = await _context.Database.ExecuteSqlInterpolatedAsync($$"""
            INSERT INTO cart_items ("UserId", "ProductId", "Quantity", "AddedAt", "CreatedAt", "UpdatedAt")
            SELECT {{userId}}, p."Id", {{quantity}}, {{now}}, {{now}}, {{now}}
            FROM products AS p
            WHERE p."Id" = {{productId}}
              AND p."IsActive" = TRUE
              AND p."Stock" >= {{quantity}}
            ON CONFLICT ("UserId", "ProductId") DO UPDATE
            SET "Quantity" = cart_items."Quantity" + EXCLUDED."Quantity",
                "UpdatedAt" = {{now}}
            WHERE cart_items."Quantity" <= {{maxQuantity}} - EXCLUDED."Quantity"
              AND cart_items."Quantity" <= (
                  SELECT p2."Stock"
                  FROM products AS p2
                  WHERE p2."Id" = {{productId}} AND p2."IsActive" = TRUE
              ) - EXCLUDED."Quantity";
            """, cancellationToken);

        return affectedRows == 1;
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

    public async Task RemovePurchasedQuantitiesAsync(
        Guid userId,
        IReadOnlyDictionary<int, int> purchasedQuantities,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        foreach (var (productId, purchasedQuantity) in purchasedQuantities)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($$"""
                DELETE FROM cart_items
                WHERE "UserId" = {{userId}}
                  AND "ProductId" = {{productId}}
                  AND "Quantity" <= {{purchasedQuantity}};

                UPDATE cart_items
                SET "Quantity" = "Quantity" - {{purchasedQuantity}}, "UpdatedAt" = {{now}}
                WHERE "UserId" = {{userId}}
                  AND "ProductId" = {{productId}}
                  AND "Quantity" > {{purchasedQuantity}};
                """, cancellationToken);
        }
    }
}
