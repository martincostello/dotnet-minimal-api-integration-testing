// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
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
        services.AddSingleton(TimeProvider.System);
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
        var group = builder.MapGroup("/api/items")
                           .RequireAuthorization();
        {
            group.MapGet("/", async (
                [AsParameters] TodoRequestContext context,
                CancellationToken cancellationToken) =>
                {
                    return await context.Service.GetListAsync(context.User, cancellationToken);
                })
                .WithSummary("Get all Todo items")
                .WithDescription("Gets all of the current user's todo items.");

            group.MapGet("/{id}", async Task<Results<Ok<TodoItemModel>, ProblemHttpResult>> (
                Guid id,
                [AsParameters] TodoRequestContext context,
                CancellationToken cancellationToken) =>
                {
                    var model = await context.Service.GetAsync(context.User, id, cancellationToken);
                    return model switch
                    {
                        null => TypedResults.Problem("Item not found.", statusCode: StatusCodes.Status404NotFound),
                        _ => TypedResults.Ok(model),
                    };
                })
                .ProducesProblem(StatusCodes.Status404NotFound)
                .WithSummary("Get a specific Todo item")
                .WithDescription("Gets the todo item with the specified ID.");

            group.MapPost("/", async Task<Results<Created<CreatedTodoItemModel>, ProblemHttpResult>> (
                CreateTodoItemModel model,
                [AsParameters] TodoRequestContext context,
                CancellationToken cancellationToken) =>
                {
                    if (string.IsNullOrWhiteSpace(model.Text))
                    {
                        return TypedResults.Problem("No item text specified.", statusCode: StatusCodes.Status400BadRequest);
                    }

                    var id = await context.Service.AddItemAsync(context.User, model.Text, cancellationToken);

                    return TypedResults.Created($"/api/items/{id}", new CreatedTodoItemModel() { Id = id });
                })
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithSummary("Create a new Todo item")
                .WithDescription("Creates a new todo item for the current user and returns its ID.");

            group.MapPost("/{id}/complete", async Task<Results<NoContent, ProblemHttpResult>> (
                Guid id,
                [AsParameters] TodoRequestContext context,
                CancellationToken cancellationToken) =>
                {
                    var wasCompleted = await context.Service.CompleteItemAsync(context.User, id, cancellationToken);

                    return wasCompleted switch
                    {
                        true => TypedResults.NoContent(),
                        false => TypedResults.Problem("Item already completed.", statusCode: StatusCodes.Status400BadRequest),
                        _ => TypedResults.Problem("Item not found.", statusCode: StatusCodes.Status404NotFound),
                    };
                })
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .WithSummary("Mark a Todo item as completed")
                .WithDescription("Marks the todo item with the specified ID as complete.");

            group.MapDelete("/{id}", async Task<Results<NoContent, ProblemHttpResult>> (
                Guid id,
                [AsParameters] TodoRequestContext context,
                CancellationToken cancellationToken) =>
                {
                    var wasDeleted = await context.Service.DeleteItemAsync(context.User, id, cancellationToken);
                    return wasDeleted switch
                    {
                        true => TypedResults.NoContent(),
                        false => TypedResults.Problem("Item not found.", statusCode: StatusCodes.Status404NotFound),
                    };
                })
                .ProducesProblem(StatusCodes.Status404NotFound)
                .WithSummary("Delete a Todo item")
                .WithDescription("Deletes the todo item with the specified ID.");
        };

        // Redirect to Open API/Swagger documentation
        builder.MapGet("/api", () => Results.Redirect("/swagger-ui/index.html"))
               .ExcludeFromDescription()
               .RequireAuthorization();

        return builder;
    }

    private record struct TodoRequestContext(TodoUser User, ITodoService Service);

    private readonly struct TodoUser
    {
        private TodoUser(string id)
        {
            Id = id;
        }

        public string Id { get; }

        public static implicit operator string(TodoUser value) => value.Id;

        public static ValueTask<TodoUser> BindAsync(HttpContext context)
        {
            return ValueTask.FromResult(new TodoUser(context.User.GetUserId()));
        }
    }
}
