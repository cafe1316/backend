using Cafe1316.Domain.Entities;
using Cafe1316.Domain.Enums;

namespace Cafe1316.Application.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int orderId, CancellationToken cancellationToken = default);
    Task<(List<Order> Orders, int TotalCount)> GetUserOrdersAsync(
        Guid userId, int page, int pageSize, OrderStatus? status, //← 枚举
        CancellationToken cancellationToken = default);
    Task<Order> CreateAsync(Order order, CancellationToken cancellationToken = default);
    Task UpdateAsync(Order order, CancellationToken cancellationToken = default); 
}