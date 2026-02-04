using Cafe1316.Application.DTOs;
using Cafe1316.Application.Interfaces;
using Cafe1316.Application.Mappings;

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
        var products = await _productRepository.GetFeaturedProductsAsync(limit, cancellationToken);
        
        return products.Select(p => p.ToListDto()).ToList();
    }
}