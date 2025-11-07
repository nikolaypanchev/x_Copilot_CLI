using Asp.Versioning;
using Microsoft.OpenApi.Models;

namespace MinimalApiApp.Configuration;

public static class SwaggerConfiguration
{
    public static IServiceCollection AddVersionedSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Minimal API - V1",
                Version = "v1",
                Description = "Version 1 of the API - Initial release with basic CRUD operations",
                Contact = new OpenApiContact
                {
                    Name = "API Support",
                    Email = "support@minimalapi.com"
                }
            });

            options.SwaggerDoc("v2", new OpenApiInfo
            {
                Title = "Minimal API - V2",
                Version = "v2",
                Description = "Version 2 of the API - Enhanced features with additional validation and caching",
                Contact = new OpenApiContact
                {
                    Name = "API Support",
                    Email = "support@minimalapi.com"
                }
            });
        });

        return services;
    }

    public static IApplicationBuilder UseVersionedSwagger(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
            options.SwaggerEndpoint("/swagger/v2/swagger.json", "API V2");
            options.RoutePrefix = "swagger";
            options.DisplayRequestDuration();
        });

        return app;
    }
}
