using Cafe1316.Application.Interfaces;
using Cafe1316.Infrastructure.Data;

namespace Cafe1316.Infrastructure.Transactions;

public class EfCoreTransactionRunner : ITransactionRunner
{
    private readonly ApplicationDbContext _context;

    public EfCoreTransactionRunner(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var result = await operation(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
