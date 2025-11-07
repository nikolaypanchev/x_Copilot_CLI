using MinimalApiApp.Models;
using MinimalApiApp.Middleware;
using Polly;
using Polly.Retry;

namespace MinimalApiApp.Services;

public class UserService : IUserService
{
    private readonly List<User> _users = new();
    private int _nextId = 1;
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly ICacheService _cacheService;
    private const string CacheKeyPrefix = "user:";
    private const string AllUsersCacheKey = "users:all";

    public UserService(ICacheService cacheService)
    {
        _cacheService = cacheService;
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
                return cachedUsers;

            var users = _users.ToList();
            await _cacheService.SetAsync(AllUsersCacheKey, users, TimeSpan.FromMinutes(5));
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
                return cachedUser;

            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                throw new NotFoundException($"User with ID {id} not found");

            await _cacheService.SetAsync(cacheKey, user, TimeSpan.FromMinutes(5));
            return user;
        });
    }

    public async Task<User> CreateUserAsync(User user)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            user.Id = _nextId++;
            user.CreatedAt = DateTime.UtcNow;
            _users.Add(user);

            await _cacheService.RemoveAsync(AllUsersCacheKey);
            await _cacheService.SetAsync($"{CacheKeyPrefix}{user.Id}", user, TimeSpan.FromMinutes(5));

            return user;
        });
    }

    public async Task<User> UpdateUserAsync(int id, User user)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var existingUser = _users.FirstOrDefault(u => u.Id == id);
            if (existingUser == null)
                throw new NotFoundException($"User with ID {id} not found");

            existingUser.Name = user.Name;
            existingUser.Email = user.Email;

            await _cacheService.RemoveAsync($"{CacheKeyPrefix}{id}");
            await _cacheService.RemoveAsync(AllUsersCacheKey);

            return existingUser;
        });
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                throw new NotFoundException($"User with ID {id} not found");

            _users.Remove(user);

            await _cacheService.RemoveAsync($"{CacheKeyPrefix}{id}");
            await _cacheService.RemoveAsync(AllUsersCacheKey);

            return true;
        });
    }
}
