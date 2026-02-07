using Cafe1316.Application.DTOs;
using Cafe1316.Domain.Entities;

namespace Cafe1316.Application.Mappings;

/// <summary>
/// 手动映射扩展方法（替代 AutoMapper）
/// </summary>
public static class MappingExtensions
{
    // ===== Product → ProductListDto =====
    public static ProductListDto ToListDto(this Product product)
    {
        return new ProductListDto
        {
            Id = product.Id,
            Name = product.Name,
            Slug = product.Slug,
            Price = product.PriceCents / 100m,
            Currency = product.Currency,
            Unit = product.Unit,
            StockStatus = GetStockStatus(product.Stock),
            CategoryName = product.Category.Name,
            SubcategoryName = product.Subcategory?.Name,
            // 产品属性
            Brand = product.Brand,
            Weight = product.Weight,
            Origin = product.Origin?.ToString(),
            RoastLevel = product.RoastLevel?.ToString(),
            MainImageUrl = product.Images
                .OrderBy(i => i.DisplayOrder)
                .FirstOrDefault()?.ImageUrl,
            
            FlavorNotes = product.FlavorNotes
                .Select(fn => fn.FlavorNote.ToString())
                .ToList(),
            // 产品标签
            Tags = product.Tags
                .Select(t => t.Tag.ToString())
                .ToList(),

            // 特殊标记
            IsFeatured = product.IsFeatured
        };
    }

    // ===== 辅助方法：计算库存状态 =====
    private static string GetStockStatus(int stock)
    {
        return stock switch
        {
            0 => "OutOfStock",
            <= 5 => "LowStock",
            _ => "InStock"
        };
    }

    // ===== Product → ProductDetailDto =====
    public static ProductDetailDto ToDetailDto(this Product product)
    {
        return new ProductDetailDto
        {
            Id = product.Id,
            Name = product.Name,
            Slug = product.Slug,
            Description = product.Description,
            Price = product.PriceCents / 100m,
            Currency = product.Currency,
            StockStatus = GetStockStatus(product.Stock),
            CategoryName = product.Category.Name,
            SubcategoryName = product.Subcategory?.Name,
            Unit = product.Unit,
            Brand = product.Brand,
            Weight = product.Weight,
            Origin = product.Origin?.ToString(),
            RoastLevel = product.RoastLevel?.ToString(),
            ProcessingMethod = product.ProcessingMethod?.ToString(),
            Altitude = product.Altitude,
            Varietals = product.Varietals,
            HarvestYear = product.HarvestYear,
            CuppingScore = product.CuppingScore,
            Material = product.Material,
            Color = product.Color,
            Size = product.Size,
            Capacity = product.Capacity,
            Specifications = product.Specifications,
            FlavorNotes = product.FlavorNotes
                .Select(fn => fn.FlavorNote.ToString())
                .ToList(),
            Tags = product.Tags
                .Select(t => t.Tag.ToString())
                .ToList(),
            ImageUrls = product.Images
                .OrderBy(i => i.DisplayOrder)
                .Select(i => i.ImageUrl)// ← 直接取 ImageUrl（已经是 string）
                .ToList(), 
            IsFeatured = product.IsFeatured
        };
    }

    // ===== (List<Product>, int) → PaginatedResult<ProductListDto> =====
    public static PaginatedResult<ProductListDto> ToPaginatedResult(this (List<Product> products, int totalCount) data, int page, int pageSize)
    {
        return new PaginatedResult<ProductListDto>
        {
            Items = data.products.Select(p => p.ToListDto()).ToList(),
            TotalCount = data.totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ===== Category → CategoryWithSubsDto =====
    public static CategoryWithSubsDto ToWithSubsDto(this Category category)
    {
        return new CategoryWithSubsDto
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            Subcategories = category.Subcategories
                .Where(s => s.IsActive == true)
                .OrderBy(s => s.DisplayOrder)
                // Repository 的 Include 过滤在某些 EF Core 版本中可能不完全可靠; 
                // Mapping 层再次确认，确保数据安全; 
                // 防御性编程 ✅
                .Select(s => new SubcategoryDto
                {
                    Id = s.Id,
                    Slug = s.Slug,
                    Name = s.Name
                }).ToList()
        };
    }
}