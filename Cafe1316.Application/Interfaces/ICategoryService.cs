using Cafe1316.Application.DTOs;

namespace Cafe1316.Application.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryWithSubsDto>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);
}
