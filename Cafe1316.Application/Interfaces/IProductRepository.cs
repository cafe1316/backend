// ===== 第 1-2 行：引入命名空间 =====
// 为什么需要？因为我们要使用 Product 实体和枚举类型
using Cafe1316.Domain.Entities;  // 引入 Product 类
using Cafe1316.Domain.Enums;     // 引入 CoffeeOrigin, RoastLevel 等枚举

// ===== 第 4 行：定义命名空间 =====
// 这个接口属于 Application 层的 Interfaces 文件夹
namespace Cafe1316.Application.Interfaces;

// ===== 第 6 行：定义接口 =====
// interface = 接口，只定义方法签名，不包含实现
// public = 公开的，其他项目可以访问
public interface IProductRepository
{
    // ===== 方法 1：根据 ID 获取产品 =====
    // Task<T> = 异步方法，返回一个 Task
    // Product? = 可能返回 null（产品不存在）
    // int id = 产品 ID 参数
    // CancellationToken = 支持取消操作（可选参数）
    // = default = 参数默认值
    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    // ===== 方法 2：根据 Slug 获取产品 =====
    // Slug = SEO 友好的 URL（如 "ethiopian-yirgacheffe"）
    Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    // ===== 方法 3：获取产品列表（核心方法）=====
    // 返回元组：(List<Product> Products, int TotalCount)
    // - Products = 当前页的产品列表
    // - TotalCount = 符合条件的总数（用于计算总页数）
    Task<(List<Product> Products, int TotalCount)> GetProductsAsync(
        // 分页
        int page=1, // 页码（默认第 1 页）
        int pageSize=20, // 每页数量（默认 20 个）

        // 分类筛选
        int? categoryId=null,  // 可选：分类 ID（? = 可为 null）
        int? subcategoryId=null,  // 可选：子分类 ID

        // 咖啡属性筛选
        CoffeeOrigin? origin = null,
        RoastLevel? roastLevel = null, 

        // 标签筛选（Other Options）
        bool? isNewArrival = null,
        bool? isOrganic = null,
        bool? isSeasonal = null,

        // 搜索
        string? searchTerm = null, 

        // 排序（只有 3 个选项）
        ProductSortOption sortBy = ProductSortOption.Default,

        CancellationToken cancellationToken = default
    );

    // ===== 方法 4：获取精选产品 =====
    // 用于首页展示推荐产品
    Task<List<Product>> GetFeaturedProductsAsync(
        int limit = 10,
        CancellationToken cancellationToken = default);
}