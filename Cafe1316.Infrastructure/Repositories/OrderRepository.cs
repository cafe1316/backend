using Cafe1316.Domain.Entities;
using Cafe1316.Application.Interfaces;
using Cafe1316.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Cafe1316.Domain.Enums;

namespace Cafe1316.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;

    public OrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<(List<Order> Orders, int TotalCount)> GetUserOrdersAsync(
        Guid userId,
        int page, int pageSize, OrderStatus? status, CancellationToken cancellationToken = default)
    {
        var query = _context.Orders
            .Where(o => o.UserId == userId);

        // 第1行：检查status是否有值
        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var orders = await query
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.CreatedAt)// 最新订单优先
                .ThenByDescending(o => o.Id)
                .Skip((page-1) * pageSize)// 跳过前面的
                .Take(pageSize)// 取这一页的
                .ToListAsync(cancellationToken);

        return (orders, totalCount);
    }

    public async Task<Order> CreateAsync(Order order, CancellationToken cancellationToken = default)
    {
        await _context.Orders.AddAsync(order, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return order;
    }

    public async Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
