// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.EntityFrameworkCore;
using NodaTime;
using TodoApp;
using TodoApp.Data;
using TodoApp.Services;

// Create the default web application builder
var builder = WebApplication.CreateBuilder(args);

// Configure the Todo repository and associated services using EFCore and SQLite
builder.Services.AddSingleton<IClock>(_ => SystemClock.Instance);
builder.Services.AddScoped<ITodoRepository, TodoRepository>();
builder.Services.AddScoped<ITodoService, TodoService>();
builder.Services.AddDbContext<TodoContext>((serviceProvider, builder) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var environment = serviceProvider.GetRequiredService<IHostEnvironment>();
    var dataDirectory = configuration["DataDirectory"];

    if (string.IsNullOrEmpty(dataDirectory) || !Path.IsPathRooted(dataDirectory))
    {
        dataDirectory = Path.Combine(environment.ContentRootPath, "App_Data");
    }

    var databaseFile = Path.Combine(dataDirectory, "TodoApp.db");

    builder.UseSqlite("Data Source=" + databaseFile);
});

// Add user authentication with GitHub as an external OAuth provider
builder.Services.AddGitHubAuthentication();

// Add Razor Pages to render the UI
builder.Services.AddRazorPages();
builder.Services.AddRouting();

// Configure OpenAPI documentation for the Todo API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Todo API", Version = "v1" });
});

// Create the app
var app = builder.Build();

// Configure error handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
}

app.UseStatusCodePagesWithReExecute("/error", "?id={0}");

// Require use of HTTPS
app.UseHsts();
app.UseHttpsRedirection();

// Add static files for JavaScript, CSS and OpenAPI
app.UseStaticFiles();

// Add authN for GitHub
app.UseAuthentication();
app.UseAuthorization();

// Add the HTTP endpoints
app.MapAuthenticationRoutes();
app.MapApiRoutes();

// Add Swagger endpoint for OpenAPI
app.UseSwagger();

// Add Razor Pages for the UI
app.MapRazorPages();

// Run the application
app.Run();
