namespace Cafe1316.Application.DTOs;

/// <summary>
/// 用户信息 DTO
/// 返回给前端的用户基本信息
/// </summary>
public class UserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty; // ← 映射自 DisplayName
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; } // ← 从 Profile.AvatarUrl
}