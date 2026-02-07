using Cafe1316.Application.DTOs;
using Cafe1316.Application.Interfaces;
using Microsoft.AspNetCore.Mvc; //这是 ASP.NET Core 的核心命名空间，包含所有 MVC/API 相关的类.

namespace Cafe1316.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    // GET: api/categories
    [HttpGet]
    public async Task<ActionResult<List<CategoryWithSubsDto>>> GetAllCategories(CancellationToken cancellationToken = default)
    {
        var result = await _categoryService.GetAllCategoriesAsync(cancellationToken);
        return Ok(result);
    }

}
