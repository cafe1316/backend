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
    private readonly IProductRepository _productRepository;
    private readonly ITransactionRunner _transactionRunner;
    private readonly IPaymentService _paymentService;
    private readonly StripeSettings _stripeSettings;

    public OrderService(IOrderRepository orderRepository,
        ICartRepository cartRepository,
        ICheckoutIntentRepository checkoutIntentRepository,
        IProductRepository productRepository,
        ITransactionRunner transactionRunner,
        IPaymentService paymentService,                      
        IOptions<StripeSettings> stripeSettings)
    {
        _orderRepository = orderRepository;
        _cartRepository = cartRepository;
        _checkoutIntentRepository = checkoutIntentRepository;
        _productRepository = productRepository;
        _transactionRunner = transactionRunner;
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
        if (page < 1)
            throw new BadRequestException("Page must be at least 1.");
        if (pageSize is < 1 or > 100)
            throw new BadRequestException("Page size must be between 1 and 100.");

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
        return $"ORD{DateTime.UtcNow:yyyyMMddHHmmssfff}{Guid.NewGuid():N}"[..32];
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
        ValidateCheckoutRequest(dto);

        // 步骤1: 获取购物车
        var cartItems = await _cartRepository.GetUserCartItemsAsync(userId, cancellationToken);
        if (!cartItems.Any())
            throw new BadRequestException("Cart is empty.");

        var unavailableItem = cartItems.FirstOrDefault(item =>
            !item.Product.IsActive ||
            item.Quantity < 1 ||
            item.Product.Stock < item.Quantity);
        if (unavailableItem != null)
            throw new BadRequestException(
                $"{unavailableItem.Product.Name} is unavailable or does not have enough stock.");
        
        // 步骤2: 计算金额（模拟运费和税费）
        var subtotalLong = cartItems.Sum(item => (long)item.Product.PriceCents * item.Quantity);
        if (subtotalLong <= 0 || subtotalLong > int.MaxValue)
            throw new BadRequestException("Cart total is outside the supported range.");

        var subtotalCents = (int)subtotalLong;
        var shippingFeeCents = 1000; // 固定 $10 AUD 运费
        var taxCents = subtotalCents / 10; // 10% GST
        var grandTotalLong = (long)subtotalCents + shippingFeeCents + taxCents;
        if (grandTotalLong > int.MaxValue)
            throw new BadRequestException("Checkout total is outside the supported range.");
        var grandTotalCents = (int)grandTotalLong;

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
            CheckoutIntentId = intent.Uuid.ToString(),
            SubtotalCents = intent.SubtotalCents,
            ShippingFeeCents = intent.ShippingFeeCents,
            TaxCents = intent.TaxCents,
            GrandTotalCents = intent.GrandTotalCents,
            Currency = intent.Currency
        };
    }

    private static void ValidateCheckoutRequest(CreateCheckoutIntentDto dto)
    {
        if (dto?.ShippingAddress == null)
            throw new BadRequestException("Shipping address is required.");

        var address = dto.ShippingAddress;
        if (string.IsNullOrWhiteSpace(address.RecipientName) || address.RecipientName.Length > 120)
            throw new BadRequestException("Recipient name is required and cannot exceed 120 characters.");
        if (string.IsNullOrWhiteSpace(address.Phone) || address.Phone.Length > 20)
            throw new BadRequestException("Phone is required and cannot exceed 20 characters.");
        if (string.IsNullOrWhiteSpace(address.AddressText) || address.AddressText.Length > 500)
            throw new BadRequestException("Address is required and cannot exceed 500 characters.");
        if (string.IsNullOrWhiteSpace(address.CountryCode) || address.CountryCode.Length != 2)
            throw new BadRequestException("Country code must contain exactly two characters.");
        if (address.Province?.Length > 100 || address.City?.Length > 100 || address.District?.Length > 100)
            throw new BadRequestException("Province, city and district cannot exceed 100 characters.");
        if (address.PostalCode?.Length > 20)
            throw new BadRequestException("Postal code cannot exceed 20 characters.");
        if (!string.Equals(dto.ShippingMethod, "Standard", StringComparison.OrdinalIgnoreCase))
            throw new BadRequestException("Unsupported shipping method.");
    }
        
    public async Task<OrderDto> ProcessPaymentSuccessAsync(
        string checkoutIntentUuid,
        string stripePaymentIntentId,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(checkoutIntentUuid, out var uuid))
            throw new BadRequestException("Invalid UUID");
        if (string.IsNullOrWhiteSpace(stripePaymentIntentId))
            throw new BadRequestException("Stripe PaymentIntent ID is required.");

        return await _transactionRunner.ExecuteAsync(async transactionToken =>
        {
            // FOR UPDATE makes concurrent webhook deliveries for this checkout wait here.
            var intent = await _checkoutIntentRepository.GetByUuidForUpdateAsync(uuid, transactionToken);
            if (intent == null)
                throw new NotFoundException("Checkout intent not found");

            // Stripe may deliver the same event more than once. Replays are successful no-ops.
            if (intent.CompletedOrderId.HasValue)
            {
                var existingOrder = await _orderRepository.GetByIdAsync(
                    intent.CompletedOrderId.Value,
                    transactionToken);
                if (existingOrder == null)
                    throw new InvalidOperationException("Completed checkout references a missing order.");
                if (!string.Equals(existingOrder.StripePaymentIntentId, stripePaymentIntentId, StringComparison.Ordinal))
                    throw new InvalidOperationException("Checkout was completed by a different Stripe payment.");

                return existingOrder.ToDto();
            }

            var snapshotItems = JsonSerializer.Deserialize<List<CartItemDto>>(intent.SelectedItems)
                ?? throw new InvalidOperationException("Checkout snapshot is invalid.");
            if (snapshotItems.Count == 0 || snapshotItems.Any(item => item.ProductId <= 0 || item.Quantity <= 0))
                throw new InvalidOperationException("Checkout snapshot does not contain valid items.");

            var purchasedQuantityTotals = snapshotItems
                .GroupBy(item => item.ProductId)
                .ToDictionary(group => group.Key, group => group.Sum(item => (long)item.Quantity));
            if (purchasedQuantityTotals.Any(item => item.Value <= 0 || item.Value > int.MaxValue))
                throw new InvalidOperationException("Checkout snapshot contains an invalid aggregate quantity.");
            var purchasedQuantities = purchasedQuantityTotals
                .ToDictionary(item => item.Key, item => (int)item.Value);

            var stockDecreased = await _productRepository.TryDecreaseStockAsync(
                purchasedQuantities,
                transactionToken);
            if (!stockDecreased)
                throw new InvalidOperationException(
                    "Paid checkout could not be fulfilled because product availability changed.");

            var paidAt = DateTime.UtcNow;
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
                PaidAt = paidAt
            };

            foreach (var item in snapshotItems)
            {
                order.OrderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    ProductSku = "SNAPSHOT",
                    ProductSlug = item.ProductSlug,
                    ImageUrl = item.MainImageUrl,
                    UnitPriceCents = decimal.ToInt32(item.Price * 100),
                    Quantity = item.Quantity,
                    LineTotalCents = decimal.ToInt32(item.Subtotal * 100),
                    Currency = item.Currency
                });
            }

            order.Payment = new Payment
            {
                Uuid = Guid.NewGuid(),
                PaymentMethod = PaymentMethod.Stripe,
                TransactionId = stripePaymentIntentId,
                AmountCents = intent.GrandTotalCents,
                Currency = intent.Currency,
                Status = PaymentStatus.Completed,
                PaidAt = paidAt,
                Order = order
            };

            await _orderRepository.CreateAsync(order, transactionToken);
            intent.CompletedOrder = order;
            await _checkoutIntentRepository.UpdateAsync(intent, transactionToken);
            await _cartRepository.RemovePurchasedQuantitiesAsync(
                intent.UserId,
                purchasedQuantities,
                transactionToken);

            return order.ToDto();
        }, cancellationToken);
    }
}
