using Cafe1316.Application.DTOs;
using Cafe1316.Application.Interfaces;
using Cafe1316.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Cafe1316.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    // ⚠️ DEPRECATED: 旧版创建订单端点已废弃，现在使用 PaymentsController 的 Stripe 支付流程
    /*
    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder(
        [FromBody] CreateOrderDto dto,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _orderService.CreateOrderAsync(userId, dto, cancellationToken);
        return Ok(result);
    }
    */


    [HttpGet]
    public async Task<ActionResult<PaginatedResult<OrderDto>>> GetOrdersAsync([FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] OrderStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var result = await _orderService.GetUserOrdersAsync(userId, page, pageSize, status, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrderByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _orderService.GetOrderByIdAsync(userId, id, cancellationToken);
        
        if (result == null)
            return NotFound(new { message = $"Order {id} not found" });
        
        return Ok(result);
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User not authenticated");
        return Guid.Parse(claim);
    }
}