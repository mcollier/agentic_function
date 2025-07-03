using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting;

// Simplified service defaults for the Claims History API
public static class Extensions
{
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        // Add basic health checks
        builder.Services.AddHealthChecks();
        
        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // Add health check endpoint in development
        if (app.Environment.IsDevelopment())
        {
            app.MapHealthChecks("/health");
        }

        return app;
    }
}