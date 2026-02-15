using Cafe1316.Application.DTOs;

namespace Cafe1316.Application.Interfaces;

/// <summary>
/// 认证服务接口
/// 负责 Google OAuth 登录和用户认证
/// </summary>
public interface IAuthService
{
    Task<AuthResponseDto> GoogleLoginAsync(string idToken);
    Task<UserDto?> GetCurrentUserAsync(Guid userId);
} 