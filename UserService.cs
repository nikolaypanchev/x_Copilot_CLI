using MinimalApiApp.Models;
using MinimalApiApp.Middleware;

namespace MinimalApiApp.Services;

public class UserService : IUserService
{
    private readonly List<User> _users = new();
    private int _nextId = 1;

    public Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return Task.FromResult<IEnumerable<User>>(_users);
    }

    public Task<User> GetUserByIdAsync(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user == null)
            throw new NotFoundException($"User with ID {id} not found");
        return Task.FromResult(user);
    }

    public Task<User> CreateUserAsync(User user)
    {
        user.Id = _nextId++;
        user.CreatedAt = DateTime.UtcNow;
        _users.Add(user);
        return Task.FromResult(user);
    }

    public Task<User> UpdateUserAsync(int id, User user)
    {
        var existingUser = _users.FirstOrDefault(u => u.Id == id);
        if (existingUser == null)
            throw new NotFoundException($"User with ID {id} not found");

        existingUser.Name = user.Name;
        existingUser.Email = user.Email;
        return Task.FromResult(existingUser);
    }

    public Task<bool> DeleteUserAsync(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user == null)
            throw new NotFoundException($"User with ID {id} not found");

        _users.Remove(user);
        return Task.FromResult(true);
    }
}
