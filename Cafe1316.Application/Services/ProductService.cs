using Cafe1316.Application.DTOs;
using Cafe1316.Application.Interfaces;
using Cafe1316.Application.Mappings;
using Cafe1316.Domain.Exceptions;

namespace Cafe1316.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    // ===== 方法 1：获取产品列表 =====
    public async Task<PaginatedResult<ProductListDto>> GetProductsAsync(
        ProductFilterParams filterParams,
        CancellationToken cancellationToken = default)
    {
        ValidateFilterParams(filterParams);

        var (products, totalCount) = await _productRepository.GetProductsAsync(filterParams, cancellationToken);

        return (products, totalCount).ToPaginatedResult(filterParams.Page, filterParams.PageSize);
    }

    // ===== 方法 2：根据 ID 获取详情 =====
    public async Task<ProductDetailDto?> GetProductByIdAsync(
        int id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        if(product == null) return null;
        return product.ToDetailDto();
    }

    // ===== 方法 3：根据 Slug 获取详情 =====
    public async Task<ProductDetailDto?> GetProductBySlugAsync(
        string slug, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetBySlugAsync(slug, cancellationToken);
        if (product == null) return null;
        return product.ToDetailDto();
    }

    // ===== 方法 4：获取精选产品 =====
    public async Task<List<ProductListDto>> GetFeaturedProductsAsync(
        int limit = 10, CancellationToken cancellationToken = default)
    {
        if (limit is < 1 or > 50)
        {
            throw new BadRequestException("Featured product limit must be between 1 and 50.");
        }

        var products = await _productRepository.GetFeaturedProductsAsync(limit, cancellationToken);
        
        return products.Select(p => p.ToListDto()).ToList();
    }

    private static void ValidateFilterParams(ProductFilterParams filterParams)
    {
        if (filterParams.Page < 1)
        {
            throw new BadRequestException("Page must be at least 1.");
        }

        if (filterParams.PageSize is < 1 or > 100)
        {
            throw new BadRequestException("Page size must be between 1 and 100.");
        }

        if (filterParams.MinPrice < 0 || filterParams.MaxPrice < 0)
        {
            throw new BadRequestException("Price filters cannot be negative.");
        }

        if (filterParams.MinPrice.HasValue &&
            filterParams.MaxPrice.HasValue &&
            filterParams.MinPrice > filterParams.MaxPrice)
        {
            throw new BadRequestException("Minimum price cannot be greater than maximum price.");
        }

        if (filterParams.SearchTerm?.Length > 100)
        {
            throw new BadRequestException("Search term cannot exceed 100 characters.");
        }
    }
}
