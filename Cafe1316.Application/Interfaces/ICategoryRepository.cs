using Cafe1316.Domain.Entities;

namespace Cafe1316.Application.Interfaces;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync(CancellationToken cancellationToken = default);
}