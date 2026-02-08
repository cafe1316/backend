namespace Cafe1316.Application.Settings;

public class JwtSettings
{
    public string Secret {get; set;} = string.Empty; //  JWT 签名密钥，用于加密和验证 Token 🔐 这是最重要的安全配置！必须是随机的、足够长的字符串（至少 32 个字符 不能泄露给任何人 生产环境要用环境变量或 Azure Key Vault 存储
    public string Issuer {get; set;} = string.Empty; // Token 的签发者（通常是您的 API 名称）验证 Token 是由您的 API 签发的，不是其他来源
    public string Audience {get; set;} = string.Empty; //  Token 的目标受众（通常是您的前端应用） 验证 Token 是为您的前端应用生成的
    public int ExpiryMinutes {get; set;} = 1440; //24hours // Token 的有效期（分钟） 用户登录后，Token 在 24 小时内有效 过期后需要重新登录 //开发环境：1440（24 小时）生产环境：60-120（1-2 小时）
}