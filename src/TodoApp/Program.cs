// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.HttpOverrides;
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
builder.Services.AddDbContext<TodoContext>((options) =>
{
    var configuration = builder.Configuration;
    var environment = builder.Environment;
    var dataDirectory = configuration["DataDirectory"];

    if (string.IsNullOrEmpty(dataDirectory) || !Path.IsPathRooted(dataDirectory))
    {
        dataDirectory = Path.Combine(environment.ContentRootPath, "App_Data");
    }

    // Ensure the configured data directory exists
    if (!Directory.Exists(dataDirectory))
    {
        Directory.CreateDirectory(dataDirectory);
    }

    var databaseFile = Path.Combine(dataDirectory, "TodoApp.db");

    options.UseSqlite("Data Source=" + databaseFile);
});

// Add user authentication with GitHub as an external OAuth provider
builder.Services.AddGitHubAuthentication();

// Add Razor Pages to render the UI
builder.Services.AddRazorPages();

// Configure OpenAPI documentation for the Todo API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Todo API", Version = "v1" });
});

if (string.Equals(builder.Configuration["CODESPACES"], "true", StringComparison.OrdinalIgnoreCase))
{
    // When running in GitHub Codespaces, X-Forwarded-Host also needs to be set
    builder.Services.Configure<ForwardedHeadersOptions>(
        options => options.ForwardedHeaders |= ForwardedHeaders.XForwardedHost);
}

// Create the app
var app = builder.Build();

// Configure error handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
}

app.UseStatusCodePagesWithReExecute("/error", "?id={0}");

// Require use of HTTPS in production
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

// Add static files for JavaScript, CSS and OpenAPI
app.UseStaticFiles();

// We explicitly call UseRouting here because of https://github.com/dotnet/aspnetcore/issues/34146
app.UseRouting();

// Add authN for GitHub
app.UseAuthentication();
app.UseAuthorization();

// Add Swagger endpoint for OpenAPI
app.UseSwagger();

// Add the HTTP endpoints
app.MapAuthenticationRoutes();
app.MapTodoApiRoutes();

// Add Razor Pages for the UI
app.MapRazorPages();

// Run the application
app.Run();
