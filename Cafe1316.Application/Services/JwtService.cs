using System.IdentityModel.Tokens.Jwt; // JWT Token 处理
using System.Security.Claims; // Claims（用户信息）
using System.Text;  
using Cafe1316.Application.Settings;
using Cafe1316.Domain.Entities;
using Cafe1316.Application.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens; // 签名密钥

namespace Cafe1316.Application.Services;

/// <summary>
/// JWT Token 服务实现
/// 负责生成包含用户信息的 JWT Token
/// </summary>
public class JwtService : IJwtService
{
    private readonly JwtSettings _jwtSettings;

    public JwtService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public string GenerateToken(User user)
    {
        // 1. 创建 Claims（用户信息）
        //Claims = 编码到 JWT 中的用户信息
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), //用户 ID
            new Claim(JwtRegisteredClaimNames.Email, user.Email), //邮箱
            new Claim(JwtRegisteredClaimNames.Name, user.DisplayName ?? user.Email), //姓名
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) //JWT ID 唯一标识符
        };

        // 2. 创建签名密钥 创建签名凭据
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);//把 Secret 字符串转换为字节数组
                    //创建对称加密密钥
                    //使用 HMAC SHA256 算法签名

        //3.创建 Token 对象（C#对象）
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,           // 谁签发的
            audience: _jwtSettings.Audience,       // 给谁用的
            claims: claims,                        // 用户信息
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),  // 过期时间
            signingCredentials: credentials        // 签名凭据
            );
        
        return new JwtSecurityTokenHandler().WriteToken(token); //把token从上面的C#对象转成字符串
    }
}