using Cafe1316.Application.DTOs;
using Cafe1316.Application.Interfaces;
using Microsoft.AspNetCore.Mvc; //这是 ASP.NET Core 的核心命名空间，包含所有 MVC/API 相关的类。

namespace Cafe1316.API.Controllers;

// ControllerBase是 Microsoft 提供的类，在 Microsoft.AspNetCore.Mvc 命名空间中
// 包含很多有用的方法：
    // - Ok() - 返回 200 状态码
    // - NotFound() - 返回 404 状态码
    // - BadRequest() - 返回 400 状态码
    // 等等...
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    //为什么用接口？
    //✅ 依赖注入的原则：依赖抽象，不依赖具体实现
    //✅ ASP.NET Core 会自动注入 ProductService的实例
    //✅ 易于测试（可以 mock 接口）
    private readonly IProductService _productServices;

    public ProductsController(IProductService productServices)
    {
        _productServices = productServices;
    }

    // GET: api/products
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<ProductListDto>>> GetProducts([FromQuery] ProductFilterParams filterParams, CancellationToken cancellationToken = default)
    {
        var result = await _productServices.GetProductsAsync(filterParams, cancellationToken);
        return Ok(result);
    
    }

    // GET: api/products/5
    [HttpGet]
    public async Task<ActionResult<ProductDetailDto?>> GetProductById(int id, CancellationToken cancellationToken = default)
    {
        var result = await _productServices.GetProductByIdAsync(id, cancellationToken);

        if (result == null)
            return NotFound(new {message = "Product not found."});
        return Ok(result);
    }

    
}