using Cafe1316.Application.DTOs;

namespace Cafe1316.Application.Interfaces;

public interface ICartService
{
    Task<CartItemDto> AddToCartAsync(Guid userId, AddToCartDto dto, CancellationToken cancellationToken = default);

    Task<CartItemDto> UpdateCartItemAsync(Guid userId, int CartItemId, UpdateCartItemDto dto, CancellationToken cancellationToken = default);

    Task<MergeCartResultDto> MergeGuestCartAsync(Guid userId, MergeCartDto dto, CancellationToken cancellationToken = default);

    Task DeleteCartItemAsync(Guid userId, int CartItemId, CancellationToken cancellationToken = default);

    Task<CartDto> GetUserCartAsync(Guid userId, CancellationToken cancellationToken = default);

    Task ClearCartAsync(Guid userId, CancellationToken cancellationToken = default);
}
