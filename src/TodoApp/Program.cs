// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.HttpOverrides;
using TodoApp;

// Create the default web application builder
var builder = WebApplication.CreateBuilder(args);

// Configure the Todo repository and associated services
builder.Services.AddTodoApi();

// Add user authentication with GitHub as an external OAuth provider
builder.Services.AddGitHubAuthentication();

// Add Razor Pages to render the UI
builder.Services.AddRazorPages();

// Configure OpenAPI documentation for the Todo API
builder.Services.AddOpenApi(options =>
{
    options.UseTransformer((document, _, _) =>
    {
        document.Info.Title = "Todo API";
        document.Info.Version = "v1";
        return Task.CompletedTask;
    });
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

// Add authN for GitHub
app.UseAuthentication();
app.UseAuthorization();

// Add endpoint for OpenAPI
app.MapOpenApi();

// Add the HTTP endpoints
app.MapAuthenticationRoutes();
app.MapTodoApiRoutes();

// Add Razor Pages for the UI
app.MapRazorPages();

// Run the application
app.Run();

public partial class Program
{
    // Expose the Program class for use with WebApplicationFactory<T>
}
