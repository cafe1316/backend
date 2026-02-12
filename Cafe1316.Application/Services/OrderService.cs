using System.Text.Json;
using Cafe1316.Application.DTOs;
using Cafe1316.Application.Interfaces;
using Cafe1316.Application.Mappings;
using Cafe1316.Domain.Entities;
using Cafe1316.Domain.Enums;
using Cafe1316.Domain.Exceptions;

namespace Cafe1316.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICartRepository _cartRepository;
    public OrderService(IOrderRepository orderRepository,
        ICartRepository cartRepository)
    {
        _orderRepository = orderRepository;
        _cartRepository = cartRepository;
    }

    public async Task<OrderDto> CreateOrderAsync(
        Guid userId, CreateOrderDto dto, CancellationToken cancellationToken = default
    )
    {
        // 1. 获取购物车商品
        var cartItems = await _cartRepository.GetUserCartItemsAsync(userId, cancellationToken);
        if (!cartItems.Any())
            throw new BadRequestException("Cart is empty");

        // 2. 计算金额
        var subtotalCents = cartItems.Sum(item => item.Product.PriceCents * item.Quantity);
        var shippingFeeCents = 1000; // 固定 $10 AUD
        var taxCents = (int)(subtotalCents * 0.1); // 10% GST
        var grandTotalCents = subtotalCents + shippingFeeCents + taxCents;

        // 3. 创建Order（Email从JWT获取，通过Controller传入）
        var order = new Order
        {
            Uuid = Guid.NewGuid(),
            OrderNumber = GenerateOrderNumber(),
            UserId = userId,
            Email = dto.Email ?? string.Empty, // 从CreateOrderDto获取
            // JSONB地址
            ShippingAddress = JsonSerializer.Serialize(dto.ShippingAddress),
            BillingAddress = dto.BillingAddress != null
                ? JsonSerializer.Serialize(dto.BillingAddress)
                : null,
            // 金额
            SubtotalCents = subtotalCents,
            ShippingFeeCents = shippingFeeCents,
            TaxCents = taxCents,
            GrandTotalCents = grandTotalCents,
            Currency = "AUD",
            Status = OrderStatus.Pending,
            Notes = dto.Notes
        };

        // 4. 创建OrderItems（保存商品快照)
        foreach (var cartItem in cartItems)
        {
            var product = cartItem.Product;
            order.OrderItems.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                ProductSku = product.Sku,
                ProductSlug = product.Slug,
                ImageUrl = product.Images
                    .OrderBy(i => i.DisplayOrder)
                    .FirstOrDefault()?.ImageUrl,
                UnitPriceCents = product.PriceCents,
                Quantity = cartItem.Quantity,
                LineTotalCents = product.PriceCents * cartItem.Quantity,
                Currency = product.Currency
            });
        }

        // 5. 保存Order
        await _orderRepository.CreateAsync(order, cancellationToken);

        // 6. 清空购物车
        await _cartRepository.ClearUserCartAsync(userId, cancellationToken);

        // 7. 返回DTO
        return order.ToDto();
    }

    public async Task<OrderDto?> GetOrderByIdAsync(Guid userId,
        int orderId,
        CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);

        if(order == null)
            return null;
        
        // 验证所有权
        if(order.UserId != userId)
            throw new ForbiddenException("You are not authorized to view this order");
        
        return order.ToDto();
    }

    public async Task<PaginatedResult<OrderDto>> GetUserOrdersAsync(
        Guid userId,
        int page,
        int pageSize,
        OrderStatus? status,
        CancellationToken cancellationToken = default)
    {
        var(orders, totalCount) = await _orderRepository.GetUserOrdersAsync(userId, page, pageSize, status, cancellationToken);

        return new PaginatedResult<OrderDto>
        {
            Items = orders.Select(o => o.ToDto()).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    private string GenerateOrderNumber()
    {
        // 格式: ORD20260210143523234
        return $"ORD{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(100, 999)}";
    }
}