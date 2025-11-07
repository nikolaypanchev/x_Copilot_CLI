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

    public UserService()
    {
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, retryAttempt))
            );
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _retryPolicy.ExecuteAsync(() => 
            Task.FromResult<IEnumerable<User>>(_users));
    }

    public async Task<User> GetUserByIdAsync(int id)
    {
        return await _retryPolicy.ExecuteAsync(() =>
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                throw new NotFoundException($"User with ID {id} not found");
            return Task.FromResult(user);
        });
    }

    public async Task<User> CreateUserAsync(User user)
    {
        return await _retryPolicy.ExecuteAsync(() =>
        {
            user.Id = _nextId++;
            user.CreatedAt = DateTime.UtcNow;
            _users.Add(user);
            return Task.FromResult(user);
        });
    }

    public async Task<User> UpdateUserAsync(int id, User user)
    {
        return await _retryPolicy.ExecuteAsync(() =>
        {
            var existingUser = _users.FirstOrDefault(u => u.Id == id);
            if (existingUser == null)
                throw new NotFoundException($"User with ID {id} not found");

            existingUser.Name = user.Name;
            existingUser.Email = user.Email;
            return Task.FromResult(existingUser);
        });
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        return await _retryPolicy.ExecuteAsync(() =>
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                throw new NotFoundException($"User with ID {id} not found");

            _users.Remove(user);
            return Task.FromResult(true);
        });
    }
}
