using Cafe1316.Application.DTOs;
using Cafe1316.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Cafe1316.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("google")]
    public async Task<ActionResult<AuthResponseDto>> GoogleLogin([FromBody] GoogleLoginDto loginDto)//[FromBody] 告诉 ASP.NET Core："从 HTTP 请求的 Body 中读取 JSON，并自动转换成这个参数的类型"
    {
        var result = await _authService.GoogleLoginAsync(loginDto.IdToken);
        return Ok(result);
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        // 暂时返回 NotImplemented
        return StatusCode(501, "Not implemented yet");
    }
}