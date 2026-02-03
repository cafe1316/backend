using Cafe1316.Domain.Enums;

namespace Cafe1316.Application.DTOs;

public class ProductFilterParams
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int? CategoryId { get; set; }
    public int? SubcategoryId { get; set; }
    public CoffeeOrigin? Origin { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public List<RoastLevel>? RoastLevels { get; set; }
    public string? SearchTerm { get; set; }
    public bool? IsNewArrival { get; set; }
    public bool? IsOrganic { get; set; }
    public bool? IsSeasonal { get; set;}
    public ProductSortOption SortBy { get; set; } = ProductSortOption.Default;
    public List<FlavorNote>? FlavorNotes { get; set; }
}