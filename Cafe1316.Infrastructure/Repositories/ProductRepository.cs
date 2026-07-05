// ===== 引入命名空间 =====
using Cafe1316.Application.Interfaces;
using Cafe1316.Application.DTOs;
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
            .AsNoTracking()
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
            .AsNoTracking()
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
            .AsNoTracking()
            .Include(p=>p.Category)
            .Include(p=>p.Subcategory)
            .Include(p=>p.Images)
            .Include(p=>p.FlavorNotes)
            .Include(p => p.Tags)
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
            var term = filterParams.SearchTerm.Trim();

            // Pre-calculate matching enum values for partial search (支持枚举的部分匹配)
            var matchingOrigins = Enum.GetValues<CoffeeOrigin>()
                .Where(e => e.ToString().Contains(term, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var lowerTerm = term.ToLower();

            query = query.Where(p =>
                p.Name.ToLower().Contains(lowerTerm) ||
                (p.Description != null && p.Description.ToLower().Contains(lowerTerm)) ||
                (p.Brand != null && p.Brand.ToLower().Contains(lowerTerm)) ||
                (p.Varietals != null && p.Varietals.ToLower().Contains(lowerTerm)) ||
                (p.Origin.HasValue && matchingOrigins.Contains(p.Origin.Value))
            );
        }

        // 2.5 标签筛选（通过 ProductTagMapping）
        if (filterParams.IsNewArrival.HasValue && filterParams.IsNewArrival.Value)
        {
            query = query.Where(p => p.Tags.Any(t => t.Tag == ProductTag.NewArrival));
        }
        if (filterParams.IsOrganic.HasValue && filterParams.IsOrganic.Value)
        {
            query = query.Where(p => p.Tags.Any(t => t.Tag == ProductTag.Organic));
        }
        if (filterParams.IsSeasonal.HasValue && filterParams.IsSeasonal.Value)
        {
            query = query.Where(p => p.Tags.Any(t => t.Tag == ProductTag.Seasonal));
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
            query = query.Where(p => p.RoastLevel.HasValue && filterParams.RoastLevels.Contains(p.RoastLevel.Value));
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
            ProductSortOption.PriceLowToHigh => query.OrderBy(p=>p.PriceCents)
                .ThenBy(p => p.Id),
            ProductSortOption.PriceHighToLow => query.OrderByDescending(p=>p.PriceCents)
                .ThenBy(p => p.Id),
            _ => query.OrderByDescending(p => p.IsFeatured)//先排精选
                .ThenByDescending(p => p.CreatedAt)//再排创建时间
                .ThenBy(p => p.Id)
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
            .AsNoTracking()
            .Include(p=>p.Category)
            .Include(p=>p.Subcategory)
            .Include(p=>p.Images)
            .Include(p=>p.FlavorNotes)
            .Where(p=>p.IsFeatured==true && p.IsActive==true)// 只查询激活且精选的产品
            .OrderByDescending(p => p.CreatedAt) // 按创建时间排序（最新的在前）
            .ThenBy(p => p.Id)
            .Take(limit)  // 限制数量
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> TryDecreaseStockAsync(
        IReadOnlyDictionary<int, int> quantities,
        CancellationToken cancellationToken = default)
    {
        foreach (var (productId, quantity) in quantities)
        {
            var affectedRows = await _context.Database.ExecuteSqlInterpolatedAsync($$"""
                UPDATE products
                SET "Stock" = "Stock" - {{quantity}},
                    "UpdatedAt" = {{DateTime.UtcNow}}
                WHERE "Id" = {{productId}}
                  AND "IsActive" = TRUE
                  AND "Stock" >= {{quantity}};
                """, cancellationToken);

            if (affectedRows != 1)
            {
                return false;
            }
        }

        return true;
    }

}
