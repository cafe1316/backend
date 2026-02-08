namespace Cafe1316.Application.DTOs;

/// <summary>
/// 认证响应 DTO
/// 用户登录成功后，后端返回给前端的数据
/// </summary>
public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    //JWT Token 字符串
    //前端用途：保存到 localStorage  每次 API 请求都带上：Authorization: Bearer {Token}
    public UserDto User { get; set; } = null!;
    //用户信息对象
    //= null! 的含义：null! = 告诉编译器"我知道这可能是 null，但我保证会赋值" 避免 nullable 警告
    //前端用途：显示用户名、头像  保存用户状态
}
