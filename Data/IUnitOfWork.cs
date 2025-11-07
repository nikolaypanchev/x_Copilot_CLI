using MinimalApiApp.Services;

namespace MinimalApiApp.Data;

// Unit of Work interface
public interface IUnitOfWork
{
    IProductRepository Products { get; }
    Task<int> CommitAsync();
}
