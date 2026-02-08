using Cafe1316.Domain.Entities;

namespace Cafe1316.Application.Interfaces;

/// <summary>
/// JWT Token 服务接口
/// 负责生成 JWT Token（每个用户独特的身份凭证）
/// </summary>
public interface IJwtService
{
//JWT Token 包含用户 ID 包含用户 Email 包含签发时间 包含唯一 ID 所以即使是同一个用户，每次登录的 Token 也不一样！
    string GenerateToken(User user);
}