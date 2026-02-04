using Cafe1316.Application.DTOs;

namespace Cafe1316.Application.Interfaces;

public interface IProductService
{
    // ===== 方法 1：获取产品列表 =====
    Task<PaginatedResult<ProductListDto>> GetProductsAsync(
    ProductFilterParams filterParams,
    CancellationToken cancellationToken = default);

    // ===== 方法 2：根据 ID 获取详情 =====
    Task<ProductDetailDto?> GetProductByIdAsync(
    int id, CancellationToken cancellationToken = default);

    // ===== 方法 3：根据 Slug 获取详情 =====
    Task<ProductDetailDto?> GetProductBySlugAsync(
    string slug, CancellationToken cancellationToken = default);

    // ===== 方法 4：获取精选产品 =====
    Task<List<ProductListDto>> GetFeaturedProductsAsync(
    int limit = 10, CancellationToken cancellationToken = default);
}