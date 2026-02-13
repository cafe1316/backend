using Cafe1316.Application.DTOs;
using Cafe1316.Application.Interfaces;
using Cafe1316.Domain.Entities;
using Cafe1316.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Cafe1316.API.Controllers;

// ⚠️ DEPRECATED: 此测试控制器使用了已废弃的 CreateOrderAsync 方法
// 如需测试订单流程，请使用 PaymentsController 的 Stripe 支付流程
/*
[ApiController]
[Route("api/test")]
public class TestOrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ICartService _cartService;
    private readonly IUserRepository _userRepository;
    private readonly IProductRepository _productRepository;

    public TestOrderController(
        IOrderService orderService,
        ICartService cartService,
        IUserRepository userRepository,
        IProductRepository productRepository)
    {
        _orderService = orderService;
        _cartService = cartService;
        _userRepository = userRepository;
        _productRepository = productRepository;
    }

    [HttpPost("run-test")]
    public async Task<ActionResult> RunTest()
    {
        try
        {
            // 1. 获取或创建一个测试用户
            var email = "test@example.com";
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                user = new User 
                { 
                    Email = email, 
                    DisplayName = "Test User",
                    FirstName = "Test",
                    LastName = "User"
                };
                user = await _userRepository.AddAsync(user);
            }
            var userId = user.Id;

            // 2. 清空购物车
            await _cartService.ClearCartAsync(userId);

            // 3. 添加商品到购物车 (尝试获取第一个可用的产品)
            var products = await _productRepository.GetProductsAsync(new ProductFilterParams { PageSize = 1 });
            if (!products.Products.Any()) return BadRequest("No products found in database. Please seed data.");
            
            var product = products.Products.First();
            await _cartService.AddToCartAsync(userId, new AddToCartDto { ProductId = product.Id, Quantity = 2 });

            // 4. 创建订单
            var address = new OrderAddressDto
            {
                RecipientName = "Test User",
                Phone = "0412345678",
                AddressText = "123 Test St",
                City = "Sydney",
                Province = "NSW",
                PostalCode = "2000",
                CountryCode = "AU"
            };

            var orderDto = new CreateOrderDto
            {
                Email = email,
                ShippingAddress = address,
                BillingAddress = address,
                Notes = "Test Order"
            };

            var order = await _orderService.CreateOrderAsync(userId, orderDto);

            // 5. 获取订单列表
            var orders = await _orderService.GetUserOrdersAsync(userId, 1, 10, null);

            // 6. 获取订单详情
            var orderDetail = await _orderService.GetOrderByIdAsync(userId, order.Id);

            return Ok(new 
            { 
                Message = "✅ Test Passed!", 
                CreatedOrderId = order.Id,
                OrderTotal = order.GrandTotal,
                UserOrdersCount = orders.TotalCount,
                OrderDetail = orderDetail
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = "❌ Test Failed", Error = ex.Message, StackTrace = ex.StackTrace });
        }
    }
}
*/
