using Cafe1316.Application.DTOs;
using Cafe1316.Domain.Exceptions;
using Cafe1316.Application.Interfaces;
using Cafe1316.Application.Mappings;
using Cafe1316.Application.Common;

namespace Cafe1316.Application.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly ITransactionRunner _transactionRunner;

    public CartService(
        ICartRepository cartRepository,
        IProductRepository productRepository,
        ITransactionRunner transactionRunner)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _transactionRunner = transactionRunner;
    }

    public async Task<CartItemDto> AddToCartAsync(Guid userId, AddToCartDto dto, CancellationToken cancellationToken = default)
    {
        var productId = dto.ProductId;
        var quantity = dto.Quantity;

        ValidateQuantity(quantity);

        var product = await _productRepository.GetByIdAsync(productId, cancellationToken) ?? throw new NotFoundException($"Product {productId} not found");

        var added = await _cartRepository.TryAddQuantityAsync(
            userId,
            productId,
            quantity,
            CartRules.MaxQuantityPerProduct,
            cancellationToken);

        if (!added)
        {
            var existing = await _cartRepository.GetByUserAndProductAsync(userId, productId, cancellationToken);
            var currentQuantity = existing?.Quantity ?? 0;
            var maxCanAdd = Math.Max(Math.Min(
                product.Stock - currentQuantity,
                CartRules.MaxQuantityPerProduct - currentQuantity), 0);

            throw new BadRequestException(
                maxCanAdd > 0
                    ? $"Can only add {maxCanAdd} more to cart."
                    : "This item is out of stock or has reached the cart limit."
            );
        }

        var savedItem = await _cartRepository.GetByUserAndProductAsync(userId, productId, cancellationToken)
            ?? throw new InvalidOperationException("Cart item was not found after a successful update.");
        return savedItem.ToDto();
    }


    public async Task<CartItemDto> UpdateCartItemAsync(Guid userId, int CartItemId, UpdateCartItemDto dto, CancellationToken cancellationToken = default)
    {
        ValidateQuantity(dto.Quantity);

        var item = await _cartRepository.GetByIdAsync(CartItemId, cancellationToken) 
                ?? throw new NotFoundException($"Cart item {CartItemId} not found");

        if (item.UserId != userId)
        {
            throw new ForbiddenException("Unauthorized");
        }

        //这里不处理把数量删减到0，直接在前端删减到0的时候调delete

        //验证库存
        if(item.Product.Stock < dto.Quantity)
            throw new BadRequestException($"Insufficient stock");

        // 直接更新（允许前端自己决定是否删除）
        item.Quantity = dto.Quantity;
        await _cartRepository.UpdateAsync(item, cancellationToken);
        return item.ToDto();
    }

    public async Task<MergeCartResultDto> MergeGuestCartAsync(Guid userId, MergeCartDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.Items is null || dto.Items.Count == 0)
        {
            return new MergeCartResultDto
            {
                Cart = await GetUserCartAsync(userId, cancellationToken)
            };
        }

        if (dto.Items.Count > CartRules.MaxMergeItems)
        {
            throw new BadRequestException($"A guest cart can contain at most {CartRules.MaxMergeItems} items.");
        }

        return await _transactionRunner.ExecuteAsync(async transactionCancellationToken =>
        {
            var rejectedItems = new List<RejectedCartItemDto>();

            foreach (var group in dto.Items.GroupBy(item => item.ProductId))
            {
                var invalidItem = group.FirstOrDefault(item =>
                    item.Quantity is < 1 or > CartRules.MaxQuantityPerProduct);
                var totalQuantity = group.Sum(item => (long)item.Quantity);

                if (invalidItem != null || totalQuantity is < 1 or > CartRules.MaxQuantityPerProduct)
                {
                    var requestedQuantity = totalQuantity > int.MaxValue
                        ? int.MaxValue
                        : totalQuantity < int.MinValue
                            ? int.MinValue
                            : (int)totalQuantity;
                    rejectedItems.Add(Reject(
                        new AddToCartDto
                        {
                            ProductId = group.Key,
                            Quantity = requestedQuantity
                        },
                        null,
                        "InvalidQuantity",
                        $"Quantity must be between 1 and {CartRules.MaxQuantityPerProduct}."));
                    continue;
                }

                var item = new AddToCartDto
                {
                    ProductId = group.Key,
                    Quantity = (int)totalQuantity
                };
                var rejection = await TryMergeGuestItemAsync(userId, item, transactionCancellationToken);
                if (rejection != null)
                {
                    rejectedItems.Add(rejection);
                }
            }

            return new MergeCartResultDto
            {
                Cart = await GetUserCartAsync(userId, transactionCancellationToken),
                RejectedItems = rejectedItems
            };
        }, cancellationToken);
    }

    private async Task<RejectedCartItemDto?> TryMergeGuestItemAsync(
        Guid userId,
        AddToCartDto item,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);
        if (product == null)
        {
            return Reject(item, null, "Unavailable", "This product is no longer available.");
        }

        var added = await _cartRepository.TryAddQuantityAsync(
            userId,
            item.ProductId,
            item.Quantity,
            CartRules.MaxQuantityPerProduct,
            cancellationToken);

        if (!added)
        {
            var existing = await _cartRepository.GetByUserAndProductAsync(userId, item.ProductId, cancellationToken);
            var currentQuantity = existing?.Quantity ?? 0;
            var availableQuantity = Math.Max(Math.Min(
                product.Stock - currentQuantity,
                CartRules.MaxQuantityPerProduct - currentQuantity), 0);
            var message = availableQuantity == 0
                ? "No additional quantity is currently available."
                : $"Only {availableQuantity} more can be added.";

            return Reject(
                item,
                product.Name,
                product.Stock == 0 ? "OutOfStock" : "InsufficientStock",
                message,
                availableQuantity);
        }

        return null;
    }

    private static RejectedCartItemDto Reject(
        AddToCartDto item,
        string? productName,
        string reason,
        string message,
        int? availableQuantity = null) => new()
    {
        ProductId = item.ProductId,
        ProductName = productName,
        RequestedQuantity = item.Quantity,
        AvailableQuantity = availableQuantity,
        Reason = reason,
        Message = message
    };

    public async Task DeleteCartItemAsync(Guid userId, int CartItemId, CancellationToken cancellationToken = default)
    {
        var item = await _cartRepository.GetByIdAsync(CartItemId, cancellationToken)
                ?? throw new NotFoundException($"Cart item {CartItemId} not found");

        if (item.UserId != userId)
        {
            throw new ForbiddenException("Unauthorized");
        }

        await _cartRepository.DeleteAsync(item, cancellationToken);
    }

    public async Task<CartDto> GetUserCartAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var items = await _cartRepository.GetUserCartItemsAsync(userId, cancellationToken);
        return items.ToCartDto();
    }

    public async Task ClearCartAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await _cartRepository.ClearUserCartAsync(userId, cancellationToken);
    }

    private static void ValidateQuantity(int quantity)
    {
        if (quantity is < 1 or > CartRules.MaxQuantityPerProduct)
        {
            throw new BadRequestException(
                $"Quantity must be between 1 and {CartRules.MaxQuantityPerProduct}.");
        }
    }

}
