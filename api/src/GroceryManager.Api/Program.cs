using GroceryManager.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices(builder.Configuration, builder.Environment);

var app = builder.Build();

await app.SeedDefaultDataAsync();
app.UseWebApplicationMiddleware();

app.Run();
