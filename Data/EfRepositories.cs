using Microsoft.EntityFrameworkCore;
using MinimalApiApp.Models;
using MinimalApiApp.Middleware;
using MinimalApiApp.Services;
using Polly;
using Polly.Retry;

namespace MinimalApiApp.Data;

// EF Core User Repository
public class EfUserRepository : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly ICacheService _cacheService;
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly ILogger<EfUserRepository> _logger;
    private const string CacheKeyPrefix = "user:";
    private const string AllUsersCacheKey = "users:all";

    public EfUserRepository(
        ApplicationDbContext context,
        ICacheService cacheService,
        ILogger<EfUserRepository> logger)
    {
        _context = context;
        _cacheService = cacheService;
        _logger = logger;
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, retryAttempt))
            );
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var cachedUsers = await _cacheService.GetAsync<List<User>>(AllUsersCacheKey);
            if (cachedUsers != null)
            {
                _logger.LogInformation("Retrieved {Count} users from cache", cachedUsers.Count);
                return cachedUsers;
            }

            var users = await _context.Users.ToListAsync();
            await _cacheService.SetAsync(AllUsersCacheKey, users, TimeSpan.FromMinutes(5));
            _logger.LogInformation("Retrieved {Count} users from database", users.Count);
            return users;
        });
    }

    public async Task<User> GetUserByIdAsync(int id)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var cacheKey = $"{CacheKeyPrefix}{id}";
            var cachedUser = await _cacheService.GetAsync<User>(cacheKey);
            if (cachedUser != null)
            {
                _logger.LogInformation("Retrieved user {UserId} from cache", id);
                return cachedUser;
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                throw new NotFoundException($"User with ID {id} not found");

            await _cacheService.SetAsync(cacheKey, user, TimeSpan.FromMinutes(5));
            _logger.LogInformation("Retrieved user {UserId} from database", id);
            return user;
        });
    }

    public async Task<User> CreateUserAsync(User user)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            user.CreatedAt = DateTime.UtcNow;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await _cacheService.RemoveAsync(AllUsersCacheKey);
            await _cacheService.SetAsync($"{CacheKeyPrefix}{user.Id}", user, TimeSpan.FromMinutes(5));

            _logger.LogInformation("Created user {UserId}", user.Id);
            return user;
        });
    }

    public async Task<User> UpdateUserAsync(int id, User user)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
                throw new NotFoundException($"User with ID {id} not found");

            existingUser.Name = user.Name;
            existingUser.Email = user.Email;

            await _context.SaveChangesAsync();

            await _cacheService.RemoveAsync($"{CacheKeyPrefix}{id}");
            await _cacheService.RemoveAsync(AllUsersCacheKey);

            _logger.LogInformation("Updated user {UserId}", id);
            return existingUser;
        });
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                throw new NotFoundException($"User with ID {id} not found");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            await _cacheService.RemoveAsync($"{CacheKeyPrefix}{id}");
            await _cacheService.RemoveAsync(AllUsersCacheKey);

            _logger.LogInformation("Deleted user {UserId}", id);
            return true;
        });
    }
}

// EF Core Product Repository
public class EfProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ICacheService _cacheService;
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly ILogger<EfProductRepository> _logger;
    private const string CacheKeyPrefix = "product:";
    private const string AllProductsCacheKey = "products:all";

    public EfProductRepository(
        ApplicationDbContext context,
        ICacheService cacheService,
        ILogger<EfProductRepository> logger)
    {
        _context = context;
        _cacheService = cacheService;
        _logger = logger;
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
            {
                _logger.LogInformation("Retrieved {Count} products from cache", cachedProducts.Count);
                return cachedProducts;
            }

            var products = await _context.Products.ToListAsync();
            await _cacheService.SetAsync(AllProductsCacheKey, products, TimeSpan.FromMinutes(5));
            _logger.LogInformation("Retrieved {Count} products from database", products.Count);
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
            {
                _logger.LogInformation("Retrieved product {ProductId} from cache", id);
                return cachedProduct;
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
                throw new NotFoundException($"Product with ID {id} not found");

            await _cacheService.SetAsync(cacheKey, product, TimeSpan.FromMinutes(5));
            _logger.LogInformation("Retrieved product {ProductId} from database", id);
            return product;
        });
    }

    public async Task<Product> AddAsync(Product product)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            await _cacheService.RemoveAsync(AllProductsCacheKey);
            await _cacheService.SetAsync($"{CacheKeyPrefix}{product.Id}", product, TimeSpan.FromMinutes(5));

            _logger.LogInformation("Created product {ProductId}", product.Id);
            return product;
        });
    }

    public async Task<Product> UpdateAsync(int id, Product product)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var existing = await _context.Products.FindAsync(id);
            if (existing == null)
                throw new NotFoundException($"Product with ID {id} not found");

            existing.Name = product.Name;
            existing.Description = product.Description;
            existing.Price = product.Price;
            existing.Stock = product.Stock;

            await _context.SaveChangesAsync();

            await _cacheService.RemoveAsync($"{CacheKeyPrefix}{id}");
            await _cacheService.RemoveAsync(AllProductsCacheKey);

            _logger.LogInformation("Updated product {ProductId}", id);
            return existing;
        });
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                throw new NotFoundException($"Product with ID {id} not found");

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            await _cacheService.RemoveAsync($"{CacheKeyPrefix}{id}");
            await _cacheService.RemoveAsync(AllProductsCacheKey);

            _logger.LogInformation("Deleted product {ProductId}", id);
            return true;
        });
    }
}
