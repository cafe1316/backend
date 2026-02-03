// ===== 引入命名空间 =====
using Cafe1316.Application.Interfaces;
using Cafe1316.Domain.Entities;
using Cafe1316.Domain.Enums;
using Cafe1316.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cafe1316.Infrastructure.Repositories;

// ===== 实现接口 =====
// : IProductRepository = 实现 IProductRepository 接口
public class ProductRepository : IProductRepository
{
    // ===== 私有字段：数据库上下文 =====
    // readonly = 只能在构造函数中赋值
    // _context = 命名约定（私有字段用下划线开头
    private readonly ApplicationDbContext _context;

    // ===== 构造函数：依赖注入 =====
    // 当创建 ProductRepository 实例时，会自动注入 ApplicationDbContext
    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;  // 保存到私有字段
    }

    // ===== 方法 1：根据 ID 获取产品 =====
    public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        // return await = 异步返回
        return await _context.Products
            .Include(p=>p.Category)
            .Include(p=>p.Subcategory)
            .Include(p=>p.Images)
            .Include(p=>p.FlavorNotes)
            .Include(p=>p.Tags)
            .Where(p=>p.Id==id && p.IsActive==true)
            .FirstOrDefaultAsync(cancellationToken);
    }

    // ===== 方法 2：根据 Slug 获取产品 =====
    public async Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p=>p.Category)
            .Include(p=>p.Subcategory)
            .Include(p=>p.Images)
            .Include(p=>p.FlavorNotes)
            .Include(p=>p.Tags)
            .Where(p=>p.Slug == slug && p.IsActive == true)
            .FirstOrDefaultAsync(cancellationToken);
    }

    // ===== 方法 3：获取产品列表（支持筛选、排序、分页）=====
    public async Task<(List<Product> Products, int TotalCount)> GetProductsAsync(ProductFilterParams filterParams, CancellationToken cancellationToken = default)
    {
        // 第 1 步：构建基础查询
        var query = _context.Products
            .Include(p=>p.Category)
            .Include(p=>p.Subcategory)
            .Include(p=>p.Images)
            .Include(p=>p.FlavorNotes)
            .Where(p=>p.IsActive==true);

        // 第 2 步：应用筛选条件
        // 2.1 分类筛选
        if (filterParams.CategoryId.HasValue)
        {
            query = query.Where(p=>p.CategoryId==filterParams.CategoryId);
        }

        // 2.2 子分类筛选
        if (filterParams.SubcategoryId.HasValue)
        {
            query = query.Where(p=>p.SubcategoryId==filterParams.SubcategoryId);
        }

        // 2.3 产地筛选
        if (filterParams.Origin.HasValue)
        {
            query = query.Where(p=>p.Origin==filterParams.Origin);
        }

        // 2.4 搜索关键词
        if (!string.IsNullOrEmpty(filterParams.SearchTerm))
        {
            query = query.Where(p=>
                p.Name.Contains(filterParams.SearchTerm) ||
                p.Description.Contains(filterParams.SearchTerm));
        }

        // 2.5 Other Options 筛选
        if (filterParams.IsNewArrival.HasValue)
        {
            query = query.Where(p => p.IsNewArrival == filterParams.IsNewArrival);
        }
        
        if (filterParams.IsOrganic.HasValue)
        {
            query = query.Where(p => p.IsOrganic == filterParams.IsOrganic);
        }

        if (filterParams.IsSeasonal.HasValue)
        {
            query = query.Where(p => p.IsSeasonal == filterParams.IsSeasonal);
        }

        // 2.6 价格区间筛选
        if (filterParams.MinPrice.HasValue)
        {
            var minPriceCents = (int)(filterParams.MinPrice.Value * 100);
            query = query.Where(p=>p.PriceCents >= minPriceCents);
        }

        if (filterParams.MaxPrice.HasValue)
        {
            var maxPriceCents = (int)(filterParams.MaxPrice.Value * 100);
            query = query.Where(p => p.PriceCents <= maxPriceCents);
        }

        // 2.7 烘焙度筛选（多选）
        if (filterParams.RoastLevels != null && filterParams.RoastLevels.Any())
        {
            query = query.Where(p=>filterParams.RoastLevels.Contains(p.RoastLevel));
        }

        // 2.8 风味标签筛选（多选）
        if (filterParams.FlavorNotes != null && filterParams.FlavorNotes.Any())
        {
            query = query.Where(p=>
                p.FlavorNotes.Any(fn=>filterParams.FlavorNotes.Contains(fn.FlavorNote)));
        }

        // 第 3 步：计算总数（在分页之前）
        var totalCount = await query.CountAsync(cancellationToken);

        // 第 4 步：应用排序
        query = filterParams.SortBy switch
        {
            ProductSortOption.PriceLowToHigh => query.OrderBy(p=>p.PriceCents),
            ProductSortOption.PriceHighToLow => query.OrderByDescending(p=>p.PriceCents),
            _ => query.OrderBy(p=>p.DisplayOrder)
        };

        // 第 5 步：应用分页
        query = query
            .Skip((filterParams.Page - 1) * filterParams.PageSize)//第一页的话（1-1） * 20 = 0，那就是skip0个从1开始；
            .Take(filterParams.PageSize); //就是这一页拿取20个展示

        // 第 6 步：执行查询
        //返回类型是 List<Product>用ToListAsync
        var products = await query.ToListAsync(cancellationToken);

        // 第 7 步：返回结果（元组）
        return (products, totalCount);
    }

    // ===== 方法 4：获取精选产品 =====
    public async Task<List<Product>> GetFeaturedProductsAsync(int limit = 10, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p=>p.Category)
            .Include(p=>p.Subcategory)
            .Include(p=>p.Images)
            .Include(p=>p.FlavorNotes)
            .Where(p=>p.IsFeatured==true && p.IsActive==true)// 只查询激活且精选的产品
            .OrderBy(p=>p.DisplayOrder) // 按推荐顺序排序
            .Take(limit)  // 限制数量
            .ToListAsync(cancellationToken);

    }

}