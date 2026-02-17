using System.Text.Json;
using Cafe1316.Application.DTOs;
using Cafe1316.Application.Interfaces;
using Cafe1316.Application.Mappings;
using Cafe1316.Domain.Entities;
using Cafe1316.Domain.Enums;
using Cafe1316.Domain.Exceptions;
using Microsoft.Extensions.Options;
using Cafe1316.Application.Settings;

namespace Cafe1316.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICartRepository _cartRepository;
    private readonly ICheckoutIntentRepository _checkoutIntentRepository;
    private readonly IPaymentService _paymentService;
    private readonly StripeSettings _stripeSettings;

    public OrderService(IOrderRepository orderRepository,
        ICartRepository cartRepository,
        ICheckoutIntentRepository checkoutIntentRepository,  
        IPaymentService paymentService,                      
        IOptions<StripeSettings> stripeSettings)
    {
        _orderRepository = orderRepository;
        _cartRepository = cartRepository;
        _checkoutIntentRepository = checkoutIntentRepository;
        _paymentService = paymentService;
        _stripeSettings = stripeSettings.Value;
    }

    // ⚠️ DEPRECATED: 旧版直接创建订单方法（已废弃，现在统一使用 Stripe 支付流程）
    // 保留代码供参考，如需支持货到付款等其他支付方式，可重新启用
    /*
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
    */


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

    public async Task<OrderDto?> GetOrderByCheckoutIntentIdAsync(Guid userId, Guid checkoutIntentId, CancellationToken cancellationToken = default)
    {
        var intent = await _checkoutIntentRepository.GetByUuidAsync(checkoutIntentId, cancellationToken);
        
        if (intent == null) return null;
        if (intent.UserId != userId) return null; // Security check
        
        if (intent.CompletedOrderId.HasValue)
        {
            return await GetOrderByIdAsync(userId, intent.CompletedOrderId.Value, cancellationToken);
        }
        
        return null;
    }

    public async Task<CheckoutResponseDto> CreateCheckoutIntentAsync(
        Guid userId, 
        CreateCheckoutIntentDto dto, 
        CancellationToken cancellationToken = default)
    {
        // 步骤1: 获取购物车
        var cartItems = await _cartRepository.GetUserCartItemsAsync(userId, cancellationToken);
        if (!cartItems.Any())
            throw new BadRequestException("Cart is Empty");
        
        // 步骤2: 计算金额（模拟运费和税费）
        var subtotalCents = cartItems.Sum(item => item.Product.PriceCents * item.Quantity);
        var shippingFeeCents = 1000; // 固定 $10 AUD 运费
        var taxCents = (int)(subtotalCents * 0.1); // 10% GST
        var grandTotalCents = subtotalCents + shippingFeeCents + taxCents;

        // 步骤3: 创建 CheckoutIntent 实体（快照）
        var intent = new CheckoutIntent
        {
            Uuid = Guid.NewGuid(),
            UserId = userId,
            SelectedItems = JsonSerializer.Serialize(cartItems.Select(i => i.ToDto())), 
            ShippingAddress = JsonSerializer.Serialize(dto.ShippingAddress),
            BillingAddress = dto.BillingAddress != null 
                ? JsonSerializer.Serialize(dto.BillingAddress) 
                : null,
            ShippingMethod = dto.ShippingMethod,
            SubtotalCents = subtotalCents,
            ShippingFeeCents = shippingFeeCents,
            TaxCents = taxCents,
            GrandTotalCents = grandTotalCents,
            Currency = "AUD",
            ExpiresAt = DateTime.UtcNow.AddHours(1) // 1小时过期
        };

        // 步骤4: 调用 Stripe 创建 PaymentIntent
        var clientSecret = await _paymentService.CreatePaymentIntentAsync(
            grandTotalCents, 
            "aud", 
            new Dictionary<string, string> 
            { 
                { "checkout_intent_uuid", intent.Uuid.ToString() } // 贴便利贴！
            }, 
            cancellationToken);

        // 步骤5: 保存到数据库
        await _checkoutIntentRepository.AddAsync(intent, cancellationToken);

        // 步骤6: 返回 DTO（补上这段）
        return new CheckoutResponseDto
        {
            ClientSecret = clientSecret,
            PublishableKey = _stripeSettings.PublishableKey,
            CheckoutIntentId = intent.Uuid.ToString()
        };
    }
        
    public async Task<OrderDto> ProcessPaymentSuccessAsync(
    string checkoutIntentUuid, 
    string stripePaymentIntentId, 
    CancellationToken cancellationToken = default)
    {
        // 步骤1: 验证并查找 Intent
        //Guid.TryParse(string, out Guid): 尝试把字符串转换成 Guid。
        //    成功 → 返回 true，并把结果赋值给 out参数
        //    失败 → 返回 false
        if (!Guid.TryParse(checkoutIntentUuid, out var uuid))
            throw new BadRequestException("Invalid UUID");
        var intent = await _checkoutIntentRepository.GetByUuidAsync(uuid, cancellationToken);
        if (intent == null)
            throw new NotFoundException("Checkout intent not found");

        if (intent.CompletedOrderId.HasValue)
            throw new BadRequestException("Order already processed, cannot duplicate");
        // 步骤2: 创建 Order
        var order = new Order
        {
            Uuid = Guid.NewGuid(),
            OrderNumber = GenerateOrderNumber(),
            UserId = intent.UserId,
            Email = intent.User.Email,
            ShippingAddress = intent.ShippingAddress!,
            BillingAddress = intent.BillingAddress,
            SubtotalCents = intent.SubtotalCents,
            ShippingFeeCents = intent.ShippingFeeCents,
            TaxCents = intent.TaxCents,
            GrandTotalCents = intent.GrandTotalCents,
            Currency = intent.Currency,
            Status = OrderStatus.Paid,
            StripePaymentIntentId = stripePaymentIntentId,
            PaidAt = DateTime.UtcNow
        };
        // 步骤3: 从快照创建 OrderItems
        var cartItemDtos = JsonSerializer.Deserialize<List<CartItemDto>>(intent.SelectedItems);
        if (cartItemDtos != null)
        {
            foreach (var item in cartItemDtos)
            {
                order.OrderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    ProductSku = "SNAPSHOT",
                    ProductSlug = item.ProductSlug,
                    ImageUrl = item.MainImageUrl,
                    UnitPriceCents = (int)(item.Price * 100),
                    Quantity = item.Quantity,
                    LineTotalCents = (int)(item.Subtotal * 100),
                    Currency = item.Currency
                });
            }
        }

        // 步骤4: 创建 Payment 记录
        var payment = new Payment
        {
            Uuid = Guid.NewGuid(),
            PaymentMethod = PaymentMethod.Stripe,
            TransactionId = stripePaymentIntentId,
            AmountCents = intent.GrandTotalCents,
            Currency = intent.Currency,
            Status = PaymentStatus.Completed,
            PaidAt = DateTime.UtcNow,
            Order = order
        };
        order.Payment = payment;
        // 步骤5: 保存 Order
        await _orderRepository.CreateAsync(order, cancellationToken);
        // 步骤6: 更新 Intent
        intent.CompletedOrder = order;
        await _checkoutIntentRepository.UpdateAsync(intent, cancellationToken);
        // 步骤7: 清空购物车
        await _cartRepository.ClearUserCartAsync(intent.UserId, cancellationToken);
        return order.ToDto();

    }
}   