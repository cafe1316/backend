namespace Cafe1316.Application.Settings;

/// <summary>
/// Google OAuth configuration settings
/// 这个类将用来存储从 appsettings.json 读取的 Google OAuth 配置
/// </summary>
public class GoogleAuthSettings 
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}
