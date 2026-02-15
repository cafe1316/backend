using Cafe1316.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Cafe1316.Application.Services;
using Cafe1316.Application.Interfaces;
using Cafe1316.Infrastructure.Repositories;
using Cafe1316.Infrastructure.Services; // 新增：用于 StripePaymentService
using Cafe1316.API.Middleware;
using Cafe1316.Application.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ===== 配置服务（Services） =====

// 配置数据库
// 配置数据库
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

        // 优先使用 Render/Heroku 提供的 DATABASE_URL
        if (!string.IsNullOrEmpty(databaseUrl))
        {
            connectionString = databaseUrl;
        }

        // 如果是 postgres:// 开头的 URI 格式 (Render 默认)，需要转换
        if (!string.IsNullOrEmpty(connectionString) && connectionString.StartsWith("postgres://"))
        {
            try 
            {
                var databaseUri = new Uri(connectionString);
                var userInfo = databaseUri.UserInfo.Split(':');
                var npgsqlBuilder = new Npgsql.NpgsqlConnectionStringBuilder
                {
                    Host = databaseUri.Host,
                    Port = databaseUri.Port,
                    Username = userInfo[0],
                    Password = userInfo[1],
                    Database = databaseUri.LocalPath.TrimStart('/')
                };
                connectionString = npgsqlBuilder.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing connection URI: {ex.Message}");
                // 转换失败则回退到原始字符串，让 Npgsql 抛出具体错误
            }
        }

        options.UseNpgsql(connectionString);
    });

// 注册 Repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
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
builder.Services.Configure<GoogleAuthSettings>(builder.Configuration.GetSection("GoogleAuth"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

// 配置 JWT 认证
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
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

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
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

// 1. 全局异常处理（必须放在最前面，捕获所有后续中间件的异常）
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 2. CORS（必须在 Authentication 之前）
app.UseCors("AllowFrontend");

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

// 4. 数据库 Seed (自动迁移和填充数据)
// 注意：在生产环境谨慎使用，已在 DbInitializer 中加入检查，防止重复填充
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    // 自动应用迁移 (如果用 Postgres)
    if (!app.Environment.IsDevelopment()) 
    {
        try {
             await context.Database.MigrateAsync(); 
        } catch (Exception ex) {
             Console.WriteLine($"Migration Error: {ex.Message}");
        }
    }
    
    // 填充种子数据
    await DbInitializer.SeedAsync(context);
}

// 4. 认证和授权（必须在 MapControllers 之前）
app.UseAuthentication();    // JWT 认证
app.UseAuthorization();     // 授权

// 5. 路由和 Controllers（必须在最后）
app.MapControllers();

// ===== 启动应用 =====

app.Run();
