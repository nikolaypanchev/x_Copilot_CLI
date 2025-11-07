using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using MinimalApiApp.Models;
using MinimalApiApp.Services;

namespace MinimalApiApp.Middleware
{
    public class EmailValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IUserService _userService;
        private readonly ILogger<EmailValidationMiddleware> _logger;
        private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public EmailValidationMiddleware(RequestDelegate next, IUserService userService, ILogger<EmailValidationMiddleware> logger)
        {
            _next = next;
            _userService = userService;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Only validate POST/PUT requests targeting /api/users
            if ((HttpMethods.IsPost(context.Request.Method) || HttpMethods.IsPut(context.Request.Method))
                && context.Request.Path.StartsWithSegments("/api/users", StringComparison.OrdinalIgnoreCase))
            {
                context.Request.EnableBuffering();

                string body;
                using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true))
                {
                    body = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0;
                }

                if (string.IsNullOrWhiteSpace(body))
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(new { error = "Request body is required." });
                    return;
                }

                User? incoming;
                try
                {
                    incoming = JsonSerializer.Deserialize<User>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to deserialize user payload.");
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(new { error = "Invalid JSON payload." });
                    return;
                }

                if (incoming is null || string.IsNullOrWhiteSpace(incoming.Email) || !EmailRegex.IsMatch(incoming.Email))
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(new { error = "A valid email is required." });
                    return;
                }

                // Check uniqueness: allow same user id for PUT; for POST any existing email is conflict
                var allUsers = await _userService.GetAllUsersAsync();
                var conflict = allUsers.Any(u => string.Equals(u.Email, incoming.Email, StringComparison.OrdinalIgnoreCase)
                                                 && (HttpMethods.IsPost(context.Request.Method) || u.Id != incoming.Id));

                if (conflict)
                {
                    context.Response.StatusCode = StatusCodes.Status409Conflict;
                    await context.Response.WriteAsJsonAsync(new { error = "Email is already in use." });
                    return;
                }
            }

            await _next(context);
        }
    }
}