// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Security.Claims;
using TodoApp.Models;
using TodoApp.Services;

namespace TodoApp;

/// <summary>
/// A class containing the HTTP endpoints for the Todo API.
/// </summary>
public static class ApiModule
{
    /// <summary>
    /// Maps the endpoints for the API.
    /// </summary>
    /// <param name="builder">The <see cref="IEndpointConventionBuilder"/>.</param>
    /// <returns>
    /// A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.
    /// </returns>
    public static IEndpointRouteBuilder MapApiRoutes(this IEndpointRouteBuilder builder)
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
        builder.MapGet("/api/items/{id}", async (
            Guid id,
            ClaimsPrincipal user,
            ITodoService service,
            CancellationToken cancellationToken) =>
            {
                var model = await service.GetAsync(user.GetUserId(), id, cancellationToken);
                return model is null ? Results.Problem("Item not found.", statusCode: StatusCodes.Status404NotFound) : Results.Json(model);
            })
            .Produces<TodoItemModel>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        // Create a new Todo item
        builder.MapPost("/api/items", async (
            CreateTodoItemModel model,
            ClaimsPrincipal user,
            ITodoService service,
            CancellationToken cancellationToken) =>
            {
                if (model is null || string.IsNullOrWhiteSpace(model.Text))
                {
                    return Results.Problem("No item text specified.", statusCode: StatusCodes.Status400BadRequest);
                }

                var id = await service.AddItemAsync(user.GetUserId(), model.Text, cancellationToken);

                return Results.Created($"/api/items/{id}", new { id });
            })
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAuthorization();

        // Mark a Todo item as completed
        builder.MapPost("/api/items/{id}/complete", async (
            Guid id,
            ClaimsPrincipal user,
            ITodoService service,
            CancellationToken cancellationToken) =>
            {
                var wasCompleted = await service.CompleteItemAsync(user.GetUserId(), id, cancellationToken);

                return wasCompleted switch
                {
                    true => Results.NoContent(),
                    false => Results.Problem("Item already completed.", statusCode: StatusCodes.Status400BadRequest),
                    _ => Results.Problem("Item not found.", statusCode: StatusCodes.Status404NotFound),
                };
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        // Delete a Todo item
        builder.MapDelete("/api/items/{id}", async (
            Guid id,
            ClaimsPrincipal user,
            ITodoService service,
            CancellationToken cancellationToken) =>
            {
                var wasDeleted = await service.DeleteItemAsync(user.GetUserId(), id, cancellationToken);
                return wasDeleted ? Results.NoContent() : Results.Problem("Item not found.", statusCode: StatusCodes.Status404NotFound);
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        // Redirect to Open API/Swagger documentation
        builder.MapGet("/api", () => Results.Redirect("/swagger-ui/index.html"))
               .RequireAuthorization()
               .SuppressApi();

        return builder;
    }
}
