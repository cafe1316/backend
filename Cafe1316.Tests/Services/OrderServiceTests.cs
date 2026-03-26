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
            _paymentServiceMock.Object,
            stripeOptions);
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

    // ========================================================================
    // Branch 1: Happy path — creates order, updates intent, clears cart
    // ========================================================================
    [Fact]
    public async Task ProcessPaymentSuccessAsync_ValidIntent_CreatesOrderAndClearsCart()
    {
        // ARRANGE
        var intent = MakeCheckoutIntent(TestIntentUuid, completedOrderId: null);

        _intentRepoMock
            .Setup(r => r.GetByUuidAsync(TestIntentUuid, It.IsAny<CancellationToken>()))
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
            .Setup(r => r.ClearUserCartAsync(TestUserId, It.IsAny<CancellationToken>()))
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

        // ASSERT — cart was cleared
        _cartRepoMock.Verify(
            r => r.ClearUserCartAsync(TestUserId, It.IsAny<CancellationToken>()),
            Times.Once,
            "Cart should be cleared after order is created");

        // ASSERT — returned DTO has correct status
        result.Should().NotBeNull();
        result.GrandTotal.Should().Be(76.00m);
    }

    // ========================================================================
    // Branch 2: Idempotency guard — intent already has a CompletedOrderId
    // ========================================================================
    [Fact]
    public async Task ProcessPaymentSuccessAsync_AlreadyProcessed_ThrowsBadRequestException()
    {
        // ARRANGE — intent already has CompletedOrderId = 42 (already processed)
        var intent = MakeCheckoutIntent(TestIntentUuid, completedOrderId: 42);

        _intentRepoMock
            .Setup(r => r.GetByUuidAsync(TestIntentUuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(intent);

        // ACT
        Func<Task> act = () => _sut.ProcessPaymentSuccessAsync(
            TestIntentUuid.ToString(),
            FakePaymentIntentId);

        // ASSERT — idempotency guard fires
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*already processed*");

        // ASSERT — no order was created, no cart was cleared
        _orderRepoMock.Verify(
            r => r.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "CreateAsync must NOT be called when order is already processed");

        _cartRepoMock.Verify(
            r => r.ClearUserCartAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
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
            .Setup(r => r.GetByUuidAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
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
}
