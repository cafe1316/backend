namespace Cafe1316.Application.DTOs;

/// <summary>
/// Google 登录请求 DTO
/// 前端通过 Google OAuth 获取 ID Token 后，发送给后端
/// </summary>
public class GoogleLoginDto
{
    public string IdToken { get; set; } = string.Empty;
}