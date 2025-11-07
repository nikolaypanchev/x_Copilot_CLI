using MinimalApiApp.Models;

namespace MinimalApiApp.Services;

// Repository interface for product operations
public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<Product> AddAsync(Product product);
    Task<Product?> UpdateAsync(int id, Product product);
    Task<bool> DeleteAsync(int id);
}

// In-memory repository implementation
public class InMemoryProductRepository : IProductRepository
{
    private readonly List<Product> _products = new();
    private int _nextId = 1;

    public Task<IEnumerable<Product>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<Product>>(_products);
    }

    public Task<Product?> GetByIdAsync(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        return Task.FromResult(product);
    }

    public Task<Product> AddAsync(Product product)
    {
        product.Id = _nextId++;
        _products.Add(product);
        return Task.FromResult(product);
    }

    public Task<Product?> UpdateAsync(int id, Product product)
    {
        var existing = _products.FirstOrDefault(p => p.Id == id);
        if (existing == null)
            return Task.FromResult<Product?>(null);

        existing.Name = product.Name;
        existing.Description = product.Description;
        existing.Price = product.Price;
        existing.Stock = product.Stock;
        return Task.FromResult<Product?>(existing);
    }

    public Task<bool> DeleteAsync(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product == null)
            return Task.FromResult(false);

        _products.Remove(product);
        return Task.FromResult(true);
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

    public Task<Product?> GetProductByIdAsync(int id)
    {
        return _unitOfWork.Products.GetByIdAsync(id);
    }

    public async Task<Product> CreateProductAsync(Product product)
    {
        var created = await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.CommitAsync();
        return created;
    }

    public async Task<Product?> UpdateProductAsync(int id, Product product)
    {
        var updated = await _unitOfWork.Products.UpdateAsync(id, product);
        if (updated != null)
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
