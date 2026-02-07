namespace Cafe1316.Application.DTOs;

public class PaginatedResult<T>
{
    // ===== 数据 =====
    public List<T> Items { get; set; } = new();

    // ===== 分页信息 =====
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }

    // ===== 计算属性 =====
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}
