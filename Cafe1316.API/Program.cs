using Amazon.Lambda.AspNetCoreServer.Hosting;
using Cafe1316.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Cafe1316.Application.Services;
using Cafe1316.Application.Interfaces;
using Cafe1316.Infrastructure.Repositories;
using Cafe1316.Infrastructure.Services; // 新增：用于 StripePaymentService
using Cafe1316.Infrastructure.Transactions;
using Cafe1316.API.Middleware;
using Cafe1316.Application.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// In Lambda, load application secrets from a namespaced SSM Parameter Store path.
// This is skipped locally, where user-secrets or environment variables are used.
var systemsManagerPath = builder.Configuration["CONFIGURATION_SSM_PATH"];
if (!string.IsNullOrWhiteSpace(systemsManagerPath))
{
    builder.Configuration.AddSystemsManager(systemsManagerPath);
}

// ===== 配置服务（Services） =====

// Configure PostgreSQL from either ASP.NET Core configuration or a platform DATABASE_URL.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration["DATABASE_URL"]
        ?? builder.Configuration.GetConnectionString("DefaultConnection");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "A PostgreSQL connection must be provided through DATABASE_URL or ConnectionStrings:DefaultConnection.");
    }

    connectionString = connectionString.Trim();

    // Render-style PostgreSQL URLs need conversion to the Npgsql key/value format.
    if (connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
        connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
    {
        try
        {
            var databaseUri = new Uri(connectionString);
            var userInfo = databaseUri.UserInfo.Split(new[] { ':' }, 2);

            if (userInfo.Length != 2)
            {
                throw new FormatException("The PostgreSQL URL must include both a username and password.");
            }

            var npgsqlBuilder = new Npgsql.NpgsqlConnectionStringBuilder
            {
                Host = databaseUri.Host,
                Port = databaseUri.Port > 0 ? databaseUri.Port : 5432,
                Username = Uri.UnescapeDataString(userInfo[0]),
                Password = Uri.UnescapeDataString(userInfo[1]),
                Database = Uri.UnescapeDataString(databaseUri.LocalPath.TrimStart('/')),
                Pooling = true,
                SslMode = Npgsql.SslMode.Require
            };
            connectionString = npgsqlBuilder.ToString();
        }
        catch (Exception ex) when (ex is UriFormatException or FormatException)
        {
            throw new InvalidOperationException("DATABASE_URL is not a valid PostgreSQL URL.", ex);
        }
    }

    options.UseNpgsql(connectionString);
});

// 注册 Repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<ITransactionRunner, EfCoreTransactionRunner>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICheckoutIntentRepository, CheckoutIntentRepository>();

// 注册 Services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, StripePaymentService>();

// 配置 Google OAuth 和 JWT Settings
builder.Services
    .AddOptions<GoogleAuthSettings>()
    .Bind(builder.Configuration.GetSection("GoogleAuth"))
    .Validate(
        settings =>
            !string.IsNullOrWhiteSpace(settings.ClientId) &&
            !settings.ClientId.StartsWith("YOUR_", StringComparison.OrdinalIgnoreCase),
        "GoogleAuth:ClientId must be configured with the Cafe1316 Google web client ID.")
    .ValidateOnStart();
builder.Services
    .AddOptions<JwtSettings>()
    .Bind(builder.Configuration.GetSection("JwtSettings"))
    .Validate(
        settings =>
            !string.IsNullOrWhiteSpace(settings.Secret) &&
            Encoding.UTF8.GetByteCount(settings.Secret) >= 32 &&
            !settings.Secret.Contains("PLACEHOLDER", StringComparison.OrdinalIgnoreCase),
        "JwtSettings:Secret must be a non-placeholder secret of at least 32 bytes.")
    .Validate(
        settings => !string.IsNullOrWhiteSpace(settings.Issuer),
        "JwtSettings:Issuer is required.")
    .Validate(
        settings => !string.IsNullOrWhiteSpace(settings.Audience),
        "JwtSettings:Audience is required.")
    .Validate(
        settings => settings.ExpiryMinutes is > 0 and <= 1440,
        "JwtSettings:ExpiryMinutes must be between 1 and 1440 minutes.")
    .ValidateOnStart();
builder.Services
    .AddOptions<StripeSettings>()
    .Bind(builder.Configuration.GetSection("Stripe"))
    .Validate(
        settings =>
            !string.IsNullOrWhiteSpace(settings.SecretKey) &&
            settings.SecretKey.StartsWith("sk_", StringComparison.Ordinal) &&
            !settings.SecretKey.Contains("Use-User-Secrets", StringComparison.OrdinalIgnoreCase),
        "Stripe:SecretKey must be configured with a Stripe secret key.")
    .Validate(
        settings =>
            !string.IsNullOrWhiteSpace(settings.PublishableKey) &&
            settings.PublishableKey.StartsWith("pk_", StringComparison.Ordinal) &&
            !settings.PublishableKey.Contains("Use-User-Secrets", StringComparison.OrdinalIgnoreCase),
        "Stripe:PublishableKey must be configured with a Stripe publishable key.")
    .Validate(
        settings =>
            !string.IsNullOrWhiteSpace(settings.WebhookSecret) &&
            settings.WebhookSecret.StartsWith("whsec_", StringComparison.Ordinal) &&
            !settings.WebhookSecret.Contains("Use-User-Secrets", StringComparison.OrdinalIgnoreCase),
        "Stripe:WebhookSecret must be configured with a Stripe webhook signing secret.")
    .ValidateOnStart();

// 配置 JWT 认证
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
    ?? throw new InvalidOperationException("JwtSettings configuration section is missing.");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
    };
});

// 注册 Controllers
builder.Services.AddControllers();
builder.Services.AddHealthChecks();

// API Gateway sends HTTP API v2 events to Lambda. This adapter converts them
// into normal ASP.NET Core requests. Outside Lambda it leaves Kestrel unchanged.
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

// Configure exact frontend origins. In AWS, override index 0 with
// Cors__AllowedOrigins__0=https://<branch>.<app-id>.amplifyapp.com.
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>()
    ?.Where(origin => !string.IsNullOrWhiteSpace(origin))
    .Select(origin => origin.TrimEnd('/'))
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray()
    ?? [];

if (allowedOrigins.Length == 0)
{
    throw new InvalidOperationException("At least one Cors:AllowedOrigins entry must be configured.");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// 配置 Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ===== 构建应用 =====

var app = builder.Build();

// ===== 配置中间件管道（顺序很重要！） =====

// 1. CORS (必须放在最前面，确保所有请求都包含 CORS 头)
app.UseCors("AllowFrontend");

// 2. 全局异常处理（必须放在最前面，捕获所有后续中间件的异常）
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 2. CORS（必须在 Authentication 之前）
// Moved to top of pipeline

// 3. Swagger（仅开发环境）
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cafe1316 API V1");
        c.RoutePrefix = "swagger";
    });
}

// Database changes are explicit in cloud environments. This prevents every
// scaled container from trying to migrate or seed the same database at startup.
var applyMigrations = builder.Configuration.GetValue<bool>("DatabaseInitialization:ApplyMigrations");
var seedDatabase = builder.Configuration.GetValue<bool>("DatabaseInitialization:Seed");

if (applyMigrations || seedDatabase)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (applyMigrations)
    {
        await context.Database.MigrateAsync();
    }

    if (seedDatabase)
    {
        await DbInitializer.SeedAsync(context);
    }
}

// 4. 认证和授权（必须在 MapControllers 之前）
app.UseAuthentication();    // JWT 认证
app.UseAuthorization();     // 授权

// CI and production smoke tests use this endpoint to verify the API.
app.MapHealthChecks("/health", new HealthCheckOptions
{
    AllowCachingResponses = false
});

// 5. 路由和 Controllers（必须在最后）
app.MapControllers();

// ===== 启动应用 =====

app.Run();
