using MinimalApiApp.Models;
using MinimalApiApp.Middleware;
using MinimalApiApp.Data;
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
    private readonly ICacheService _cacheService;
    private const string CacheKeyPrefix = "product:";
    private const string AllProductsCacheKey = "products:all";

    public InMemoryProductRepository(ICacheService cacheService)
    {
        _cacheService = cacheService;
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, retryAttempt))
            );
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var cachedProducts = await _cacheService.GetAsync<List<Product>>(AllProductsCacheKey);
            if (cachedProducts != null)
                return cachedProducts;

            var products = _products.ToList();
            await _cacheService.SetAsync(AllProductsCacheKey, products, TimeSpan.FromMinutes(5));
            return products;
        });
    }

    public async Task<Product> GetByIdAsync(int id)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var cacheKey = $"{CacheKeyPrefix}{id}";
            var cachedProduct = await _cacheService.GetAsync<Product>(cacheKey);
            if (cachedProduct != null)
                return cachedProduct;

            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null)
                throw new NotFoundException($"Product with ID {id} not found");

            await _cacheService.SetAsync(cacheKey, product, TimeSpan.FromMinutes(5));
            return product;
        });
    }

    public async Task<Product> AddAsync(Product product)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            product.Id = _nextId++;
            _products.Add(product);

            await _cacheService.RemoveAsync(AllProductsCacheKey);
            await _cacheService.SetAsync($"{CacheKeyPrefix}{product.Id}", product, TimeSpan.FromMinutes(5));

            return product;
        });
    }

    public async Task<Product> UpdateAsync(int id, Product product)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var existing = _products.FirstOrDefault(p => p.Id == id);
            if (existing == null)
                throw new NotFoundException($"Product with ID {id} not found");

            existing.Name = product.Name;
            existing.Description = product.Description;
            existing.Price = product.Price;
            existing.Stock = product.Stock;

            await _cacheService.RemoveAsync($"{CacheKeyPrefix}{id}");
            await _cacheService.RemoveAsync(AllProductsCacheKey);

            return existing;
        });
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null)
                throw new NotFoundException($"Product with ID {id} not found");

            _products.Remove(product);

            await _cacheService.RemoveAsync($"{CacheKeyPrefix}{id}");
            await _cacheService.RemoveAsync(AllProductsCacheKey);

            return true;
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
