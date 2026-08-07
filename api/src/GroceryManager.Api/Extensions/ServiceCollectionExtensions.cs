using GroceryManager.Api.Persistence;
using GroceryManager.Api.Entities.Identity;
using GroceryManager.Api.Services.Documents;
using GroceryManager.Api.Services.Identity;
using GroceryManager.Api.Services.InventoryHistory;
using GroceryManager.Api.Services.Pantry;
using GroceryManager.Api.Services.Shopping;
using GroceryManager.Api.Services.ShoppingPresets;
using GroceryManager.Api.Services.Stocktakes;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;

namespace GroceryManager.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddProblemDetails();
        services.AddControllers();
        services.AddOpenApi();
        services.AddHealthChecks();
        services.AddHttpLogging(options =>
            options.LoggingFields = HttpLoggingFields.RequestMethod |
                                    HttpLoggingFields.RequestPath |
                                    HttpLoggingFields.ResponseStatusCode |
                                    HttpLoggingFields.Duration);
        services.Configure<ForwardedHeadersOptions>(options =>
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                                       ForwardedHeaders.XForwardedProto);
        services.AddCors(options =>
        {
            options.AddPolicy("WebClient", policy =>
            {
                var allowedOrigins = configuration
                    .GetSection("Cors:AllowedOrigins")
                    .Get<string[]>() ?? [];

                if (allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                }
            });
        });
        services.AddRequestTimeouts(options =>
        {
            options.DefaultPolicy = new RequestTimeoutPolicy
            {
                Timeout = TimeSpan.FromSeconds(30),
                TimeoutStatusCode = StatusCodes.Status503ServiceUnavailable
            };
        });
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 120,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));
        });
        services.AddDbContext<GroceryManagerDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        services.AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<GroceryManagerDbContext>();
        services.AddAuthentication(IdentityConstants.ApplicationScheme)
            .AddIdentityCookies();
        services.AddAuthorization();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserContext, CurrentUserContext>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IPantryService, PantryService>();
        services.AddScoped<IPantryItemService, PantryItemService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IStorageLocationService, StorageLocationService>();
        services.AddScoped<IItemTemplateService, ItemTemplateService>();
        services.AddScoped<IShoppingPresetService, ShoppingPresetService>();
        services.AddScoped<IStocktakeService, StocktakeService>();
        services.AddScoped<IShoppingListService, ShoppingListService>();
        services.AddScoped<IInventoryAdjustmentService, InventoryAdjustmentService>();
        services.AddScoped<IShoppingListDocumentService, ShoppingListDocumentService>();

        return services;
    }
}
