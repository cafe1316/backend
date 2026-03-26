using Cafe1316.Application.DTOs;
using Cafe1316.Application.Interfaces;
using Cafe1316.Application.Services;
using Cafe1316.Domain.Entities;
using Cafe1316.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace Cafe1316.Tests.Services;

public class CartServiceTests
{
    // ── Shared mocks and SUT ──────────────────────────────────────────────────
    private readonly Mock<ICartRepository>    _cartRepoMock    = new();
    private readonly Mock<IProductRepository> _productRepoMock = new();
    private readonly CartService              _sut;

    private static readonly Guid UserId = Guid.NewGuid();

    public CartServiceTests()
    {
        _sut = new CartService(_cartRepoMock.Object, _productRepoMock.Object);
    }

    // ── Helper: build a minimal Product with navigation collections ──────────
    private static Product MakeProduct(int id, int stock, int priceCents = 2000) =>
        new()
        {
            Id         = id,
            Name       = $"Product {id}",
            Slug       = $"product-{id}",
            Stock      = stock,
            PriceCents = priceCents,
            Currency   = "AUD",
            Unit       = "bag",
            // Required navigation collections so ToDto() doesn't throw
            Category   = new Category { Name = "Coffee Beans", Slug = "coffee-beans",
                                        Subcategories = new List<Subcategory>() },
            Images     = new List<ProductImage>(),
            FlavorNotes = new List<ProductFlavorNote>(),
            Tags       = new List<ProductTagMapping>(),
            CartItems  = new List<CartItem>()
        };

    // ── Helper: build a CartItem with its Product nav-prop loaded ────────────
    private static CartItem MakeCartItem(int cartItemId, Product product, int qty) =>
        new()
        {
            Id        = cartItemId,
            UserId    = UserId,
            ProductId = product.Id,
            Product   = product,
            Quantity  = qty,
            AddedAt   = DateTime.UtcNow
        };

    // ========================================================================
    // Branch 1: Product does not exist → NotFoundException
    // ========================================================================
    [Fact]
    public async Task AddToCartAsync_ProductNotFound_ThrowsNotFoundException()
    {
        // ARRANGE
        _productRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var dto = new AddToCartDto { ProductId = 99, Quantity = 1 };

        // ACT
        Func<Task> act = () => _sut.AddToCartAsync(UserId, dto);

        // ASSERT
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*99*");
    }

    // ========================================================================
    // Branch 2a: Item NOT in cart yet, adding more than stock → BadRequestException
    // ========================================================================
    [Fact]
    public async Task AddToCartAsync_ExceedsStock_NoExistingItem_ThrowsBadRequestException()
    {
        // ARRANGE — product exists with stock = 2, no existing cart item
        var product = MakeProduct(id: 1, stock: 2);

        _productRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _cartRepoMock
            .Setup(r => r.GetByUserAndProductAsync(UserId, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CartItem?)null); // Not in cart yet

        // Try to add 5, but stock is only 2
        var dto = new AddToCartDto { ProductId = 1, Quantity = 5 };

        // ACT
        Func<Task> act = () => _sut.AddToCartAsync(UserId, dto);

        // ASSERT — "Can only add 2 more to cart."
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*Can only add 2 more*");
    }

    // ========================================================================
    // Branch 2b: Item already in cart, adding more would exceed stock → "out of stock"
    // ========================================================================
    [Fact]
    public async Task AddToCartAsync_ExceedsStock_ExistingItemFillsStock_ThrowsOutOfStockMessage()
    {
        // ARRANGE — stock = 3, already has 3 in cart (fully saturated)
        var product  = MakeProduct(id: 2, stock: 3);
        var existing = MakeCartItem(cartItemId: 10, product: product, qty: 3);

        _productRepoMock
            .Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _cartRepoMock
            .Setup(r => r.GetByUserAndProductAsync(UserId, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing); // cart already full

        var dto = new AddToCartDto { ProductId = 2, Quantity = 1 };

        // ACT
        Func<Task> act = () => _sut.AddToCartAsync(UserId, dto);

        // ASSERT — maxCanAdd = 0, so "This item is out of stock."
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*out of stock*");
    }

    // ========================================================================
    // Branch 3: Item already in cart → UpdateAsync called, AddAsync NOT called
    // ========================================================================
    [Fact]
    public async Task AddToCartAsync_ItemAlreadyInCart_UpdatesQuantity_NotAddNew()
    {
        // ARRANGE — product has 10 in stock, 2 already in cart
        var product  = MakeProduct(id: 3, stock: 10);
        var existing = MakeCartItem(cartItemId: 20, product: product, qty: 2);

        _productRepoMock
            .Setup(r => r.GetByIdAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _cartRepoMock
            .Setup(r => r.GetByUserAndProductAsync(UserId, 3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _cartRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<CartItem>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var dto = new AddToCartDto { ProductId = 3, Quantity = 3 };

        // ACT
        await _sut.AddToCartAsync(UserId, dto);

        // ASSERT — UpdateAsync called with quantity = 2 + 3 = 5
        _cartRepoMock.Verify(
            r => r.UpdateAsync(
                It.Is<CartItem>(c => c.Quantity == 5),
                It.IsAny<CancellationToken>()),
            Times.Once,
            "UpdateAsync should be called once with the accumulated quantity");

        _cartRepoMock.Verify(
            r => r.AddAsync(It.IsAny<CartItem>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "AddAsync must NOT be called when item already exists");
    }

    // ========================================================================
    // Branch 4: Brand-new item → AddAsync called, UpdateAsync NOT called
    // ========================================================================
    [Fact]
    public async Task AddToCartAsync_NewItem_CallsAddAsync_NotUpdate()
    {
        // ARRANGE — product exists, cart is empty for this product
        var product       = MakeProduct(id: 4, stock: 5);
        var createdItem   = MakeCartItem(cartItemId: 30, product: product, qty: 1);

        _productRepoMock
            .Setup(r => r.GetByIdAsync(4, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // First call: GetByUserAndProductAsync returns null (not in cart yet)
        // Second call: after AddAsync, service re-fetches to get the saved entity
        _cartRepoMock
            .SetupSequence(r => r.GetByUserAndProductAsync(UserId, 4, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CartItem?)null)   // First call — item doesn't exist yet
            .ReturnsAsync(createdItem);      // Second call — after AddAsync, now exists

        _cartRepoMock
            .Setup(r => r.AddAsync(It.IsAny<CartItem>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdItem);

        var dto = new AddToCartDto { ProductId = 4, Quantity = 1 };

        // ACT
        var result = await _sut.AddToCartAsync(UserId, dto);

        // ASSERT
        _cartRepoMock.Verify(
            r => r.AddAsync(It.IsAny<CartItem>(), It.IsAny<CancellationToken>()),
            Times.Once,
            "AddAsync must be called exactly once for a new item");

        _cartRepoMock.Verify(
            r => r.UpdateAsync(It.IsAny<CartItem>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "UpdateAsync must NOT be called when adding a brand-new item");

        result.ProductId.Should().Be(4);
        result.Quantity.Should().Be(1);
    }
}
