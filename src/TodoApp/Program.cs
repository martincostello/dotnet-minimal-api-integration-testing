// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.IO;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NodaTime;
using TodoApp;
using TodoApp.Data;
using TodoApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IClock>((_) => SystemClock.Instance);
builder.Services.AddScoped<ITodoRepository, TodoRepository>();
builder.Services.AddScoped<ITodoService, TodoService>();

// TODO Remove if ClaimsPrincipal support is added to minimal actions.
// See https://github.com/dotnet/aspnetcore/issues/33870
// and https://github.com/dotnet/aspnetcore/pull/33883.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ClaimsPrincipal>((p) =>
{
    var context = p.GetRequiredService<IHttpContextAccessor>();
    return context.HttpContext!.User;
});

builder.Services.AddRouting();

builder.Services.AddGitHubAuthentication();

builder.Services.AddRazorPages();

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

builder.Services.AddHttpClient();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
}

app.UseStatusCodePagesWithReExecute("/error", "?id={0}");

app.UseHsts();
app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints((endpoints) =>
{
    endpoints.MapApiRoutes();
    endpoints.MapAuthenticationRoutes();
    endpoints.MapRazorPages();
});

app.Run();
