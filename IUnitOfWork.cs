namespace MinimalApiApp.Services;

// Unit of Work interface
public interface IUnitOfWork
{
    IProductRepository Products { get; }
    Task<int> CommitAsync();
}
