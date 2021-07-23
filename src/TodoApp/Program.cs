// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using TodoApp;
using TodoApp.Data;
using TodoApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IClock>(_ => SystemClock.Instance);
builder.Services.AddScoped<ITodoRepository, TodoRepository>();
builder.Services.AddScoped<ITodoService, TodoService>();

builder.Services.AddGitHubAuthentication();
builder.Services.AddHttpClient();
builder.Services.AddRazorPages();
builder.Services.AddRouting();
builder.Services.AddEndpointsApiExplorer();

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

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Todo API", Version = "v1" });

    options.DocInclusionPredicate(
        (_, description) => !description.ActionDescriptor.EndpointMetadata.OfType<ApiExplorerSettingsAttribute>().Any((p) => p.IgnoreApi));
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
}

app.UseStatusCodePagesWithReExecute("/error", "?id={0}");

app.UseHsts();
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthenticationRoutes();
app.MapApiRoutes();
app.UseSwagger();

app.MapRazorPages();

app.Run();
