using Cafe1316.Application.DTOs;
using Cafe1316.Domain.Enums;

namespace Cafe1316.Application.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(Guid userId, CreateOrderDto dto, CancellationToken cancellationToken = default);
    Task<OrderDto?> GetOrderByIdAsync(Guid userId, int orderId, CancellationToken cancellationToken = default);
    Task<PaginatedResult<OrderDto>> GetUserOrdersAsync(
        Guid userId, int page, int pageSize, OrderStatus? status,
        CancellationToken cancellationToken = default);
}
