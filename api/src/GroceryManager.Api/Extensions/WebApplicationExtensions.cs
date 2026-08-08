using GroceryManager.Api.Persistence;
using Scalar.AspNetCore;

namespace GroceryManager.Api.Extensions;

public static class WebApplicationExtensions
{
    public static async Task SeedDefaultDataAsync(
        this WebApplication app,
        CancellationToken cancellationToken = default)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<GroceryManagerDbContext>();
        await DefaultDataSeeder.SeedGlobalDataAsync(db, cancellationToken);
    }

    public static WebApplication UseWebApplicationMiddleware(this WebApplication app)
    {
        app.UseForwardedHeaders();
        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }
        else
        {
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.Use(async (context, next) =>
        {
            context.Response.Headers["X-Trace-Id"] = context.TraceIdentifier;

            using (app.Logger.BeginScope(new Dictionary<string, object>
                   {
                       ["TraceId"] = context.TraceIdentifier
                   }))
            {
                await next(context);
            }
        });
        app.UseHttpLogging();
        app.Use(async (context, next) =>
        {
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            context.Response.Headers["X-Frame-Options"] = "DENY";
            context.Response.Headers["Referrer-Policy"] = "no-referrer";
            context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
            await next(context);
        });
        app.UseRouting();
        app.UseCors("WebClient");
        app.UseRequestTimeouts();
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapHealthChecks("/health");

        return app;
    }
}
