using Cafe1316.Application.DTOs;
using Cafe1316.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Cafe1316.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<ActionResult<CartDto>> GetCart(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _cartService.GetUserCartAsync(userId, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CartItemDto>> AddToCart([FromBody] AddToCartDto dto, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _cartService.AddToCartAsync(userId, dto, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CartItemDto>> UpdateCartItem(int id, [FromBody] UpdateCartItemDto dto, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _cartService.UpdateCartItemAsync(userId, id, dto, cancellationToken);
        return Ok(result);
    }

    [HttpPost("merge")]
    public async Task<ActionResult<MergeCartResultDto>> MergeGuestCart([FromBody] MergeCartDto dto, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _cartService.MergeGuestCartAsync(userId, dto, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCartItem(int id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await _cartService.DeleteCartItemAsync(userId, id, cancellationToken);
        return NoContent();
    }

    [HttpDelete]
    public async Task<ActionResult> ClearCart(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        await _cartService.ClearCartAsync(userId, cancellationToken);
        return NoContent();
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User not authenticated");
        return Guid.Parse(claim);
    }

}
