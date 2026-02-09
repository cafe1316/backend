using Cafe1316.Application.DTOs;
using Cafe1316.Domain.Exceptions;
using Cafe1316.Application.Interfaces;
using Cafe1316.Domain.Entities;
using Cafe1316.Application.Mappings;

namespace Cafe1316.Application.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;

    public CartService(ICartRepository cartRepository, IProductRepository productRepository)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
    }

    public async Task<CartItemDto> AddToCartAsync(Guid userId, AddToCartDto dto, CancellationToken cancellationToken = default)
    {
        var productId = dto.ProductId;
        var quantity = dto.Quantity;

        var product = await _productRepository.GetByIdAsync(productId, cancellationToken) ?? throw new NotFoundException($"Product {productId} not found");

        var existing = await _cartRepository.GetByUserAndProductAsync(userId, productId, cancellationToken);
        
        var currentQuantity = existing?.Quantity ?? 0;
        var finalQuantity = currentQuantity + quantity;

        if (finalQuantity > product.Stock)
        {
            var maxCanAdd = product.Stock - currentQuantity;
            throw new BadRequestException(
                    maxCanAdd > 0
                        ? $"Can only add {maxCanAdd} more to cart." 
                        : "This item is out of stock."
            );
        }

        if (existing != null)
        {
            existing.Quantity = finalQuantity;
            await _cartRepository.UpdateAsync(existing, cancellationToken);
            return existing.ToDto();
        }

        //如果不存在
        var cartItem = new CartItem
        {
            UserId = userId,
            ProductId = productId,
            Quantity = quantity,
            AddedAt = DateTime.UtcNow
        };

        await _cartRepository.AddAsync(cartItem, cancellationToken); //此时导航属性都没有值，还是为null

        var added = await _cartRepository.GetByUserAndProductAsync(userId, productId, cancellationToken); //查询一遍，为导航属性赋值
        return added!.ToDto(); //!的意思是我保证绝对不是null，所以这里的意思是安全，刚添加的肯定存在
    }


    public async Task<CartItemDto> UpdateCartItemAsync(Guid userId, int CartItemId, AddToCartDto dto, CancellationToken cancellationToken = default)
    {
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

}

