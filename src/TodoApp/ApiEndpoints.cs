// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using TodoApp.Data;
using TodoApp.Models;
using TodoApp.Services;

namespace TodoApp;

/// <summary>
/// A class containing the HTTP endpoints for the Todo API.
/// </summary>
public static class ApiEndpoints
{
    /// <summary>
    /// Adds the services for the Todo API to the application.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <returns>
    /// A <see cref="IServiceCollection"/> that can be used to further configure the application.
    /// </returns>
    public static IServiceCollection AddTodoApi(this IServiceCollection services)
    {
        services.AddSingleton<IClock>(_ => SystemClock.Instance);
        services.AddScoped<ITodoRepository, TodoRepository>();
        services.AddScoped<ITodoService, TodoService>();

        services.AddDbContext<TodoContext>((serviceProvider, options) =>
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var dataDirectory = configuration["DataDirectory"];

            if (string.IsNullOrEmpty(dataDirectory) || !Path.IsPathRooted(dataDirectory))
            {
                var environment = serviceProvider.GetRequiredService<IHostEnvironment>();
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

        return services;
    }

    /// <summary>
    /// Maps the endpoints for the Todo API.
    /// </summary>
    /// <param name="builder">The <see cref="IEndpointConventionBuilder"/>.</param>
    /// <returns>
    /// A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.
    /// </returns>
    public static IEndpointRouteBuilder MapTodoApiRoutes(this IEndpointRouteBuilder builder)
    {
        // Get all Todo items
        builder.MapGet("/api/items", async (
            ITodoService service,
            ClaimsPrincipal user,
            CancellationToken cancellationToken) =>
            {
                return await service.GetListAsync(user.GetUserId(), cancellationToken);
            })
            .RequireAuthorization();

        // Get a specific Todo item
        builder.MapGet("/api/items/{id}", async Task<Results<Ok<TodoItemModel>, ProblemHttpResult>> (
            Guid id,
            ClaimsPrincipal user,
            ITodoService service,
            CancellationToken cancellationToken) =>
            {
                var model = await service.GetAsync(user.GetUserId(), id, cancellationToken);
                return model is null ? TypedResults.Problem("Item not found.", statusCode: StatusCodes.Status404NotFound) : TypedResults.Ok(model);
            })
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        // Create a new Todo item
        builder.MapPost("/api/items", async Task<Results<Created<CreatedTodoItemModel>, ProblemHttpResult>> (
            CreateTodoItemModel model,
            ClaimsPrincipal user,
            ITodoService service,
            CancellationToken cancellationToken) =>
            {
                if (string.IsNullOrWhiteSpace(model.Text))
                {
                    return TypedResults.Problem("No item text specified.", statusCode: StatusCodes.Status400BadRequest);
                }

                var id = await service.AddItemAsync(user.GetUserId(), model.Text, cancellationToken);

                return TypedResults.Created($"/api/items/{id}", new CreatedTodoItemModel() { Id = id });
            })
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAuthorization();

        // Mark a Todo item as completed
        builder.MapPost("/api/items/{id}/complete", async Task<Results<NoContent, ProblemHttpResult>> (
            Guid id,
            ClaimsPrincipal user,
            ITodoService service,
            CancellationToken cancellationToken) =>
            {
                var wasCompleted = await service.CompleteItemAsync(user.GetUserId(), id, cancellationToken);

                return wasCompleted switch
                {
                    true => TypedResults.NoContent(),
                    false => TypedResults.Problem("Item already completed.", statusCode: StatusCodes.Status400BadRequest),
                    _ => TypedResults.Problem("Item not found.", statusCode: StatusCodes.Status404NotFound),
                };
            })
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        // Delete a Todo item
        builder.MapDelete("/api/items/{id}", async Task<Results<NoContent, ProblemHttpResult>> (
            Guid id,
            ClaimsPrincipal user,
            ITodoService service,
            CancellationToken cancellationToken) =>
            {
                var wasDeleted = await service.DeleteItemAsync(user.GetUserId(), id, cancellationToken);
                return wasDeleted ? TypedResults.NoContent() : TypedResults.Problem("Item not found.", statusCode: StatusCodes.Status404NotFound);
            })
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        // Redirect to Open API/Swagger documentation
        builder.MapGet("/api", () => Results.Redirect("/swagger-ui/index.html"))
               .ExcludeFromDescription()
               .RequireAuthorization();

        return builder;
    }
}
