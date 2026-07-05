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
    private readonly Mock<ITransactionRunner> _transactionRunnerMock = new();
    private readonly CartService              _sut;

    private static readonly Guid UserId = Guid.NewGuid();

    public CartServiceTests()
    {
        _transactionRunnerMock
            .Setup(r => r.ExecuteAsync(
                It.IsAny<Func<CancellationToken, Task<MergeCartResultDto>>>(),
                It.IsAny<CancellationToken>()))
            .Returns((Func<CancellationToken, Task<MergeCartResultDto>> operation, CancellationToken token) =>
                operation(token));

        _sut = new CartService(_cartRepoMock.Object, _productRepoMock.Object, _transactionRunnerMock.Object);
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

    [Fact]
    public async Task AddToCartAsync_QuantityBelowOne_ThrowsBadRequestException()
    {
        var dto = new AddToCartDto { ProductId = 1, Quantity = 0 };

        Func<Task> act = () => _sut.AddToCartAsync(UserId, dto);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*between 1 and 99*");
    }

    [Theory]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public async Task AddToCartAsync_QuantityAboveLimit_ThrowsBadRequestException(int quantity)
    {
        var dto = new AddToCartDto { ProductId = 1, Quantity = quantity };

        Func<Task> act = () => _sut.AddToCartAsync(UserId, dto);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*between 1 and 99*");
        _cartRepoMock.Verify(r => r.TryAddQuantityAsync(
            It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateCartItemAsync_QuantityBelowOne_ThrowsBadRequestException()
    {
        var dto = new UpdateCartItemDto { Quantity = 0 };

        Func<Task> act = () => _sut.UpdateCartItemAsync(UserId, 1, dto);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*between 1 and 99*");
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
        _cartRepoMock
            .Setup(r => r.TryAddQuantityAsync(UserId, 1, 5, 99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

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
        _cartRepoMock
            .Setup(r => r.TryAddQuantityAsync(UserId, 2, 1, 99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

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
            .Setup(r => r.TryAddQuantityAsync(UserId, 3, 3, 99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        existing.Quantity = 5;

        var dto = new AddToCartDto { ProductId = 3, Quantity = 3 };

        // ACT
        await _sut.AddToCartAsync(UserId, dto);

        // ASSERT — the repository performs one atomic increment
        _cartRepoMock.Verify(
            r => r.TryAddQuantityAsync(UserId, 3, 3, 99, It.IsAny<CancellationToken>()),
            Times.Once,
            "the quantity increment must be atomic");
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

        _cartRepoMock
            .Setup(r => r.TryAddQuantityAsync(UserId, 4, 1, 99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _cartRepoMock
            .Setup(r => r.GetByUserAndProductAsync(UserId, 4, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdItem);

        var dto = new AddToCartDto { ProductId = 4, Quantity = 1 };

        // ACT
        var result = await _sut.AddToCartAsync(UserId, dto);

        // ASSERT
        _cartRepoMock.Verify(
            r => r.TryAddQuantityAsync(UserId, 4, 1, 99, It.IsAny<CancellationToken>()),
            Times.Once,
            "new items must use the same atomic upsert operation");

        result.ProductId.Should().Be(4);
        result.Quantity.Should().Be(1);
    }

    [Fact]
    public async Task MergeGuestCartAsync_MergesValidItemsAndReturnsRejectedItems()
    {
        var validProduct = MakeProduct(id: 1, stock: 5);
        var persistedItems = new List<CartItem>();

        _productRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validProduct);
        _productRepoMock
            .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        _cartRepoMock
            .Setup(r => r.TryAddQuantityAsync(UserId, 1, 2, 99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .Callback(() => persistedItems.Add(MakeCartItem(50, validProduct, 2)));
        _cartRepoMock
            .Setup(r => r.GetUserCartItemsAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(persistedItems);

        var result = await _sut.MergeGuestCartAsync(UserId, new MergeCartDto
        {
            Items =
            [
                new AddToCartDto { ProductId = 1, Quantity = 2 },
                new AddToCartDto { ProductId = 99, Quantity = 1 }
            ]
        });

        result.Cart.Items.Should().ContainSingle(item => item.ProductId == 1 && item.Quantity == 2);
        result.RejectedItems.Should().ContainSingle(item =>
            item.ProductId == 99 && item.Reason == "Unavailable");
    }

    [Fact]
    public async Task MergeGuestCartAsync_InsufficientStockRejectsOnlyThatItem()
    {
        var product = MakeProduct(id: 2, stock: 2);

        _productRepoMock
            .Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _cartRepoMock
            .Setup(r => r.GetByUserAndProductAsync(UserId, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CartItem?)null);
        _cartRepoMock
            .Setup(r => r.TryAddQuantityAsync(UserId, 2, 5, 99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _cartRepoMock
            .Setup(r => r.GetUserCartItemsAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CartItem>());

        var result = await _sut.MergeGuestCartAsync(UserId, new MergeCartDto
        {
            Items = [new AddToCartDto { ProductId = 2, Quantity = 5 }]
        });

        result.Cart.Items.Should().BeEmpty();
        result.RejectedItems.Should().ContainSingle(item =>
            item.ProductId == 2 &&
            item.Reason == "InsufficientStock" &&
            item.AvailableQuantity == 2);
        _cartRepoMock.Verify(
            r => r.TryAddQuantityAsync(UserId, 2, 5, 99, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateCartItemAsync_QuantityAboveLimit_ThrowsBadRequestException()
    {
        var dto = new UpdateCartItemDto { Quantity = 100 };

        Func<Task> act = () => _sut.UpdateCartItemAsync(UserId, 1, dto);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*between 1 and 99*");
    }

    [Fact]
    public async Task MergeGuestCartAsync_TooManyItems_ThrowsBadRequestException()
    {
        var dto = new MergeCartDto
        {
            Items = Enumerable.Range(1, 101)
                .Select(id => new AddToCartDto { ProductId = id, Quantity = 1 })
                .ToList()
        };

        Func<Task> act = () => _sut.MergeGuestCartAsync(UserId, dto);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*at most 100 items*");
        _transactionRunnerMock.Verify(r => r.ExecuteAsync(
            It.IsAny<Func<CancellationToken, Task<MergeCartResultDto>>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task MergeGuestCartAsync_DuplicateProducts_AggregatesQuantityOnce()
    {
        var product = MakeProduct(id: 8, stock: 10);
        var persistedItems = new List<CartItem>();
        _productRepoMock
            .Setup(r => r.GetByIdAsync(8, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _cartRepoMock
            .Setup(r => r.TryAddQuantityAsync(UserId, 8, 5, 99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .Callback(() => persistedItems.Add(MakeCartItem(80, product, 5)));
        _cartRepoMock
            .Setup(r => r.GetUserCartItemsAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(persistedItems);

        var result = await _sut.MergeGuestCartAsync(UserId, new MergeCartDto
        {
            Items =
            [
                new AddToCartDto { ProductId = 8, Quantity = 2 },
                new AddToCartDto { ProductId = 8, Quantity = 3 }
            ]
        });

        result.Cart.Items.Should().ContainSingle(item => item.ProductId == 8 && item.Quantity == 5);
        _cartRepoMock.Verify(
            r => r.TryAddQuantityAsync(UserId, 8, 5, 99, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateCartItemAsync_DifferentOwner_ThrowsForbiddenException()
    {
        var product = MakeProduct(id: 9, stock: 10);
        var item = MakeCartItem(90, product, 1);
        item.UserId = Guid.NewGuid();
        _cartRepoMock
            .Setup(r => r.GetByIdAsync(90, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        Func<Task> act = () => _sut.UpdateCartItemAsync(
            UserId, 90, new UpdateCartItemDto { Quantity = 2 });

        await act.Should().ThrowAsync<ForbiddenException>();
        _cartRepoMock.Verify(
            r => r.UpdateAsync(It.IsAny<CartItem>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteCartItemAsync_DifferentOwner_ThrowsForbiddenException()
    {
        var product = MakeProduct(id: 10, stock: 10);
        var item = MakeCartItem(100, product, 1);
        item.UserId = Guid.NewGuid();
        _cartRepoMock
            .Setup(r => r.GetByIdAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        Func<Task> act = () => _sut.DeleteCartItemAsync(UserId, 100);

        await act.Should().ThrowAsync<ForbiddenException>();
        _cartRepoMock.Verify(
            r => r.DeleteAsync(It.IsAny<CartItem>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task MergeGuestCartAsync_AggregatedQuantityAboveLimit_ReturnsRejectedItem()
    {
        _cartRepoMock
            .Setup(r => r.GetUserCartItemsAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CartItem>());

        var result = await _sut.MergeGuestCartAsync(UserId, new MergeCartDto
        {
            Items =
            [
                new AddToCartDto { ProductId = 11, Quantity = 60 },
                new AddToCartDto { ProductId = 11, Quantity = 40 }
            ]
        });

        result.RejectedItems.Should().ContainSingle(item =>
            item.ProductId == 11 && item.Reason == "InvalidQuantity");
        _cartRepoMock.Verify(r => r.TryAddQuantityAsync(
            It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetUserCartAsync_UnavailableItem_FlagsCartAsNotReadyForCheckout()
    {
        var product = MakeProduct(id: 12, stock: 1);
        var item = MakeCartItem(120, product, 2);
        _cartRepoMock
            .Setup(r => r.GetUserCartItemsAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([item]);

        var result = await _sut.GetUserCartAsync(UserId);

        result.HasUnavailableItems.Should().BeTrue();
        result.Items.Should().ContainSingle(cartItem =>
            !cartItem.IsAvailable && cartItem.AvailabilityMessage != null);
    }
}
