using MinimalApiApp.Models;
using MinimalApiApp.Middleware;
using Polly;
using Polly.Retry;

namespace MinimalApiApp.Services;

// Repository interface for product operations
public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product> GetByIdAsync(int id);
    Task<Product> AddAsync(Product product);
    Task<Product> UpdateAsync(int id, Product product);
    Task<bool> DeleteAsync(int id);
}

// In-memory repository implementation
public class InMemoryProductRepository : IProductRepository
{
    private readonly List<Product> _products = new();
    private int _nextId = 1;
    private readonly AsyncRetryPolicy _retryPolicy;

    public InMemoryProductRepository()
    {
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, retryAttempt))
            );
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _retryPolicy.ExecuteAsync(() => 
            Task.FromResult<IEnumerable<Product>>(_products));
    }

    public async Task<Product> GetByIdAsync(int id)
    {
        return await _retryPolicy.ExecuteAsync(() =>
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null)
                throw new NotFoundException($"Product with ID {id} not found");
            return Task.FromResult(product);
        });
    }

    public async Task<Product> AddAsync(Product product)
    {
        return await _retryPolicy.ExecuteAsync(() =>
        {
            product.Id = _nextId++;
            _products.Add(product);
            return Task.FromResult(product);
        });
    }

    public async Task<Product> UpdateAsync(int id, Product product)
    {
        return await _retryPolicy.ExecuteAsync(() =>
        {
            var existing = _products.FirstOrDefault(p => p.Id == id);
            if (existing == null)
                throw new NotFoundException($"Product with ID {id} not found");

            existing.Name = product.Name;
            existing.Description = product.Description;
            existing.Price = product.Price;
            existing.Stock = product.Stock;
            return Task.FromResult(existing);
        });
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _retryPolicy.ExecuteAsync(() =>
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null)
                throw new NotFoundException($"Product with ID {id} not found");

            _products.Remove(product);
            return Task.FromResult(true);
        });
    }
}

// Product service updated to use UnitOfWork
public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        return _unitOfWork.Products.GetAllAsync();
    }

    public Task<Product> GetProductByIdAsync(int id)
    {
        return _unitOfWork.Products.GetByIdAsync(id);
    }

    public async Task<Product> CreateProductAsync(Product product)
    {
        var created = await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.CommitAsync();
        return created;
    }

    public async Task<Product> UpdateProductAsync(int id, Product product)
    {
        var updated = await _unitOfWork.Products.UpdateAsync(id, product);
        await _unitOfWork.CommitAsync();
        return updated;
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var removed = await _unitOfWork.Products.DeleteAsync(id);
        if (removed)
            await _unitOfWork.CommitAsync();
        return removed;
    }
}
