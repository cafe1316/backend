namespace Cafe1316.Application.DTOs;

/// <summary>
/// 用户信息 DTO
/// 返回给前端的用户基本信息
/// </summary>
public class UserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty; // DisplayName
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    
    // Profile Fields
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Gender { get; set; }
    public string? Phone { get; set; }
    public string? Bio { get; set; }
    public string? BirthDate { get; set; } // Simplified as string for DTO
    public DateTime CreatedAt { get; set; }
}