using GroceryManager.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

await app.SeedDefaultDataAsync();
app.UseWebApplicationMiddleware();

app.Run();
