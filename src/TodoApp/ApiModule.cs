// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using TodoApp.Models;
using TodoApp.Services;

namespace TodoApp
{
    public static class ApiModule
    {
        public static IEndpointRouteBuilder MapApiRoutes(this IEndpointRouteBuilder builder)
        {
            // Get all Todo items
            builder.MapGet("/api/items", async (
                ITodoService service,
                ClaimsPrincipal user,
                CancellationToken cancellationToken) => await service.GetListAsync(user.GetUserId(), cancellationToken))
                .RequireAuthorization();

            // Get a specific Todo item
            builder.MapGet("/api/items/{id}", async (
                [FromRoute] Guid id,
                ClaimsPrincipal user,
                ITodoService service,
                CancellationToken cancellationToken) =>
            {
                var model = await service.GetAsync(user.GetUserId(), id, cancellationToken);
                return model is null ? Results.NotFound() : Results.Json(model);
            }).RequireAuthorization();

            // Create a new Todo item
            builder.MapPost("/api/items", async (
                [FromBody] CreateTodoItemModel model,
                ClaimsPrincipal user,
                ITodoService service,
                CancellationToken cancellationToken) =>
            {
                if (model is null || string.IsNullOrWhiteSpace(model.Text))
                {
                    return Results.BadRequest();
                }

                var id = await service.AddItemAsync(user.GetUserId(), model.Text, cancellationToken);

                return Results.Created($"/api/items/{id}", new { id });
            }).RequireAuthorization();

            // Mark a Todo item as completed
            builder.MapPost("/api/items/{id}/complete", async (
                [FromRoute] Guid id,
                ClaimsPrincipal user,
                ITodoService service,
                CancellationToken cancellationToken) =>
            {
                var wasCompleted = await service.CompleteItemAsync(user.GetUserId(), id, cancellationToken);

                return wasCompleted switch
                {
                    true => Results.NoContent(),
                    false => Results.BadRequest(),
                    _ => Results.NotFound(),
                };
            }).RequireAuthorization();

            // Delete a Todo item
            builder.MapDelete("/api/items/{id}", async (
                [FromRoute] Guid id,
                ClaimsPrincipal user,
                ITodoService service,
                CancellationToken cancellationToken) =>
            {
                var wasDeleted = await service.DeleteItemAsync(user.GetUserId(), id, cancellationToken);
                return wasDeleted ? Results.NoContent() : Results.NotFound();
            }).RequireAuthorization();

            return builder;
        }
    }
}
