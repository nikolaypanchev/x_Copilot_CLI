namespace MinimalApiApp.Services;

// Simple Unit of Work implementation for the in-memory repository
public class UnitOfWork : IUnitOfWork
{
    public IProductRepository Products { get; }

    public UnitOfWork(IProductRepository? productRepository = null)
    {
        Products = productRepository ?? new InMemoryProductRepository();
    }

    // In a real implementation this would persist transaction/changes.
    // For in-memory, it's a no-op that returns 0 (number of affected records).
    public Task<int> CommitAsync()
    {
        return Task.FromResult(0);
    }
}
