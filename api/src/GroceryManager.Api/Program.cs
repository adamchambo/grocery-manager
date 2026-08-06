using GroceryManager.Api.Persistence;
using GroceryManager.Modules.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GroceryManager.Api.Configuration;
using Scalar.AspNetCore;
using GroceryManager.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

app.UseWebApplicationMiddleware();

app.Run();
