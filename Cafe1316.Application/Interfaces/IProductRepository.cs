using Cafe1316.Domain.Entities;
using Cafe1316.Domain.Enums;

namespace Cafe1316.Application.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<(List<Product> Products, int TotalCount)> GetProductsAsync(
        int page=1,
        int pageSize=20,
        int? categoryId=null,
        int? subcategoryId=null,
        CoffeeOrigin? origin = null,
        RoastLevel? roastLevel = null, 
        bool? isFeatured = null,
        string? searchTerm = null, 
        CancellationToken cancellationToken = default
    );
}