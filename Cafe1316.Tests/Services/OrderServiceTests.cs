using System.Text.Json;
using Cafe1316.Application.DTOs;
using Cafe1316.Application.Interfaces;
using Cafe1316.Application.Services;
using Cafe1316.Application.Settings;
using Cafe1316.Domain.Entities;
using Cafe1316.Domain.Enums;
using Cafe1316.Domain.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace Cafe1316.Tests.Services;

public class OrderServiceTests
{
    // ── Shared mocks and SUT ──────────────────────────────────────────────────
    private readonly Mock<IOrderRepository>          _orderRepoMock          = new();
    private readonly Mock<ICartRepository>           _cartRepoMock           = new();
    private readonly Mock<ICheckoutIntentRepository> _intentRepoMock         = new();
    private readonly Mock<IProductRepository>         _productRepoMock        = new();
    private readonly Mock<ITransactionRunner>         _transactionRunnerMock  = new();
    private readonly Mock<IPaymentService>           _paymentServiceMock     = new();
    private readonly OrderService                    _sut;

    private static readonly Guid   TestUserId    = Guid.NewGuid();
    private static readonly Guid   TestIntentUuid = Guid.NewGuid();
    private const  string          FakePaymentIntentId = "pi_test_abc123";

    public OrderServiceTests()
    {
        // IOptions<StripeSettings> wrapping
        var stripeOptions = Options.Create(new StripeSettings
        {
            SecretKey    = "sk_test_fake",
            PublishableKey = "pk_test_fake",
            WebhookSecret  = "whsec_fake"
        });

        _sut = new OrderService(
            _orderRepoMock.Object,
            _cartRepoMock.Object,
            _intentRepoMock.Object,
            _productRepoMock.Object,
            _transactionRunnerMock.Object,
            _paymentServiceMock.Object,
            stripeOptions);

        _transactionRunnerMock
            .Setup(r => r.ExecuteAsync(
                It.IsAny<Func<CancellationToken, Task<OrderDto>>>(),
                It.IsAny<CancellationToken>()))
            .Returns((Func<CancellationToken, Task<OrderDto>> operation, CancellationToken token) => operation(token));

        _productRepoMock
            .Setup(r => r.TryDecreaseStockAsync(
                It.IsAny<IReadOnlyDictionary<int, int>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    // ── Helper: build a minimal CheckoutIntent with User nav-prop loaded ──────
    private static CheckoutIntent MakeCheckoutIntent(
        Guid uuid,
        int? completedOrderId = null,
        string email         = "test@cafe1316.com")
    {
        // Build one CartItemDto and serialize it as the SelectedItems snapshot
        var snapshot = JsonSerializer.Serialize(new List<CartItemDto>
        {
            new()
            {
                Id          = 1,
                ProductId   = 10,
                ProductName = "Ethiopia Yirgacheffe",
                ProductSlug = "ethiopia-yirgacheffe",
                Price       = 30.00m,
                Currency    = "AUD",
                Quantity    = 2,
                Subtotal    = 60.00m,
                MainImageUrl = null,
                StockStatus  = "InStock",
                AddedAt      = DateTime.UtcNow
            }
        });

        return new CheckoutIntent
        {
            Id               = 1,
            Uuid             = uuid,
            UserId           = TestUserId,
            SelectedItems    = snapshot,
            ShippingAddress  = JsonSerializer.Serialize(new { street = "123 Test St", city = "Sydney" }),
            SubtotalCents    = 6000,
            ShippingFeeCents = 1000,
            TaxCents         = 600,
            GrandTotalCents  = 7600,
            Currency         = "AUD",
            CompletedOrderId = completedOrderId,
            ExpiresAt        = DateTime.UtcNow.AddHours(1),
            // User navigation property — needed by intent.User.Email
            User = new User
            {
                Id        = TestUserId,
                Email     = email,
                FirstName = "Test",
                LastName  = "User"
            }
        };
    }

    private static CreateCheckoutIntentDto MakeCheckoutRequest() => new()
    {
        ShippingMethod = "Standard",
        ShippingAddress = new OrderAddressDto
        {
            RecipientName = "Test User",
            Phone = "0400000000",
            Province = "VIC",
            City = "Melbourne",
            District = "CBD",
            AddressText = "123 Test Street",
            PostalCode = "3000",
            CountryCode = "AU"
        }
    };

    private static CartItem MakeCartItem(bool isActive = true, int stock = 5, int quantity = 2) => new()
    {
        Id = 1,
        UserId = TestUserId,
        ProductId = 10,
        Quantity = quantity,
        Product = new Product
        {
            Id = 10,
            Name = "Ethiopia Yirgacheffe",
            Slug = "ethiopia-yirgacheffe",
            Sku = "ETH-001",
            PriceCents = 3000,
            Currency = "AUD",
            Stock = stock,
            IsActive = isActive
        }
    };

    // ========================================================================
    // Branch 1: Happy path — creates order, updates intent, clears cart
    // ========================================================================
    [Fact]
    public async Task ProcessPaymentSuccessAsync_ValidIntent_CreatesOrderAndRemovesPurchasedQuantities()
    {
        // ARRANGE
        var intent = MakeCheckoutIntent(TestIntentUuid, completedOrderId: null);

        _intentRepoMock
            .Setup(r => r.GetByUuidForUpdateAsync(TestIntentUuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(intent);

        // orderRepo.CreateAsync returns the order with an Id
        _orderRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order o, CancellationToken _) =>
            {
                o.Id = 99; // simulate DB-generated Id
                o.ShippingAddress = intent.ShippingAddress!; // ensure ToDto() can deserialize
                return o;
            });

        _intentRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<CheckoutIntent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _cartRepoMock
            .Setup(r => r.RemovePurchasedQuantitiesAsync(
                TestUserId,
                It.IsAny<IReadOnlyDictionary<int, int>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // ACT
        var result = await _sut.ProcessPaymentSuccessAsync(
            TestIntentUuid.ToString(),
            FakePaymentIntentId);

        // ASSERT — order was created
        _orderRepoMock.Verify(
            r => r.CreateAsync(
                It.Is<Order>(o =>
                    o.UserId          == TestUserId &&
                    o.GrandTotalCents == 7600       &&
                    o.Status          == OrderStatus.Paid &&
                    o.StripePaymentIntentId == FakePaymentIntentId),
                It.IsAny<CancellationToken>()),
            Times.Once,
            "CreateAsync should be called once with correct order data");

        // ASSERT — intent was marked as completed
        _intentRepoMock.Verify(
            r => r.UpdateAsync(
                It.Is<CheckoutIntent>(i => i.CompletedOrder != null),
                It.IsAny<CancellationToken>()),
            Times.Once,
            "Intent UpdateAsync should be called to mark it as completed");

        // ASSERT — only quantities from this checkout snapshot are removed
        _cartRepoMock.Verify(
            r => r.RemovePurchasedQuantitiesAsync(
                TestUserId,
                It.Is<IReadOnlyDictionary<int, int>>(items => items[10] == 2),
                It.IsAny<CancellationToken>()),
            Times.Once,
            "Only purchased quantities should be removed after order creation");

        // ASSERT — returned DTO has correct status
        result.Should().NotBeNull();
        result.GrandTotal.Should().Be(76.00m);
    }

    // ========================================================================
    // Branch 2: Idempotency guard — intent already has a CompletedOrderId
    // ========================================================================
    [Fact]
    public async Task ProcessPaymentSuccessAsync_AlreadyProcessed_ReturnsExistingOrder()
    {
        // ARRANGE — intent already has CompletedOrderId = 42 (already processed)
        var intent = MakeCheckoutIntent(TestIntentUuid, completedOrderId: 42);

        _intentRepoMock
            .Setup(r => r.GetByUuidForUpdateAsync(TestIntentUuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(intent);

        var existingOrder = new Order
        {
            Id = 42,
            UserId = TestUserId,
            OrderNumber = "ORD-EXISTING",
            Email = "test@cafe1316.com",
            ShippingAddress = JsonSerializer.Serialize(new OrderAddressDto
            {
                RecipientName = "Test User",
                Phone = "0400000000",
                AddressText = "123 Test St",
                CountryCode = "AU"
            }),
            Currency = "AUD",
            StripePaymentIntentId = FakePaymentIntentId,
            Status = OrderStatus.Paid
        };
        _orderRepoMock
            .Setup(r => r.GetByIdAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingOrder);

        var result = await _sut.ProcessPaymentSuccessAsync(
            TestIntentUuid.ToString(),
            FakePaymentIntentId);

        result.Id.Should().Be(42);

        // ASSERT — no order was created, no cart was cleared
        _orderRepoMock.Verify(
            r => r.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "CreateAsync must NOT be called when order is already processed");

        _cartRepoMock.Verify(
            r => r.RemovePurchasedQuantitiesAsync(
                It.IsAny<Guid>(),
                It.IsAny<IReadOnlyDictionary<int, int>>(),
                It.IsAny<CancellationToken>()),
            Times.Never,
            "ClearUserCartAsync must NOT be called for a duplicate webhook");
    }

    // ========================================================================
    // Branch 3: CheckoutIntent not found → NotFoundException
    // ========================================================================
    [Fact]
    public async Task ProcessPaymentSuccessAsync_IntentNotFound_ThrowsNotFoundException()
    {
        // ARRANGE — repo returns null for any UUID
        _intentRepoMock
            .Setup(r => r.GetByUuidForUpdateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CheckoutIntent?)null);

        var randomUuid = Guid.NewGuid().ToString();

        // ACT
        Func<Task> act = () => _sut.ProcessPaymentSuccessAsync(randomUuid, FakePaymentIntentId);

        // ASSERT
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Checkout intent not found*");
    }

    // ========================================================================
    // Branch 4: Malformed UUID string → BadRequestException
    // ========================================================================
    [Fact]
    public async Task ProcessPaymentSuccessAsync_InvalidUuid_ThrowsBadRequestException()
    {
        // ACT
        Func<Task> act = () => _sut.ProcessPaymentSuccessAsync("not-a-valid-uuid", FakePaymentIntentId);

        // ASSERT
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*Invalid UUID*");
    }

    [Fact]
    public async Task CreateCheckoutIntentAsync_AvailableCart_UsesServerCalculatedAmount()
    {
        _cartRepoMock
            .Setup(r => r.GetUserCartItemsAsync(TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CartItem> { MakeCartItem() });
        _paymentServiceMock
            .Setup(s => s.CreatePaymentIntentAsync(
                7600,
                "aud",
                It.Is<Dictionary<string, string>>(metadata => metadata.ContainsKey("checkout_intent_uuid")),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("client_secret_test");
        _intentRepoMock
            .Setup(r => r.AddAsync(It.IsAny<CheckoutIntent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CheckoutIntent intent, CancellationToken _) => intent);

        var result = await _sut.CreateCheckoutIntentAsync(TestUserId, MakeCheckoutRequest());

        result.SubtotalCents.Should().Be(6000);
        result.GrandTotalCents.Should().Be(7600);
        result.ClientSecret.Should().Be("client_secret_test");
    }

    [Fact]
    public async Task CreateCheckoutIntentAsync_InsufficientStock_RejectsBeforeStripeCall()
    {
        _cartRepoMock
            .Setup(r => r.GetUserCartItemsAsync(TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CartItem> { MakeCartItem(stock: 1, quantity: 2) });

        Func<Task> act = () => _sut.CreateCheckoutIntentAsync(TestUserId, MakeCheckoutRequest());

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*enough stock*");
        _paymentServiceMock.Verify(
            s => s.CreatePaymentIntentAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateCheckoutIntentAsync_InvalidAddress_RejectsBeforeLoadingCart()
    {
        var request = MakeCheckoutRequest();
        request.ShippingAddress.RecipientName = "";

        Func<Task> act = () => _sut.CreateCheckoutIntentAsync(TestUserId, request);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*Recipient name*");
        _cartRepoMock.Verify(
            r => r.GetUserCartItemsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessPaymentSuccessAsync_StockChanged_DoesNotCreateOrder()
    {
        var intent = MakeCheckoutIntent(TestIntentUuid);
        _intentRepoMock
            .Setup(r => r.GetByUuidForUpdateAsync(TestIntentUuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(intent);
        _productRepoMock
            .Setup(r => r.TryDecreaseStockAsync(
                It.IsAny<IReadOnlyDictionary<int, int>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        Func<Task> act = () => _sut.ProcessPaymentSuccessAsync(
            TestIntentUuid.ToString(),
            FakePaymentIntentId);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*availability changed*");
        _orderRepoMock.Verify(
            r => r.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public async Task GetUserOrdersAsync_InvalidPagination_Rejects(int page, int pageSize)
    {
        Func<Task> act = () => _sut.GetUserOrdersAsync(
            TestUserId,
            page,
            pageSize,
            null);

        await act.Should().ThrowAsync<BadRequestException>();
    }
}
