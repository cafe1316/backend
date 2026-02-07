using Cafe1316.Application.Interfaces;
using Cafe1316.Domain.Entities;
using Cafe1316.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cafe1316.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _context;

    public CategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .Include(c => c.Subcategories
                    .Where(s => s.IsActive == true))
            .Where(c => c.IsActive == true)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(cancellationToken);
    }
}