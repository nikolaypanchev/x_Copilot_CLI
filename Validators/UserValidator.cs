using FluentValidation;
using MinimalApiApp.Models;
using MinimalApiApp.Services;

namespace MinimalApiApp.Validators;

public class UserValidator : AbstractValidator<User>
{
    private readonly IUserService _userService;

    public UserValidator(IUserService userService)
    {
        _userService = userService;

        RuleFor(x => x.Name)
            .NotNull()
            .WithMessage("User name cannot be null")
            .NotEmpty()
            .WithMessage("User name is required");

        RuleFor(x => x.Email)
            .NotNull()
            .WithMessage("Email cannot be null")
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("A valid email is required")
            .MustAsync(BeUniqueEmail)
            .WithMessage("Email is already in use");
    }

    private async Task<bool> BeUniqueEmail(User user, string email, CancellationToken cancellationToken)
    {
        var allUsers = await _userService.GetAllUsersAsync();
        var conflict = allUsers.Any(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase)
                                         && u.Id != user.Id);
        return !conflict;
    }
}
