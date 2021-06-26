// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
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
                CancellationToken cancellationToken) =>
            {
                return await service.GetListAsync(user.GetUserId(), cancellationToken);
            }).RequireAuthorization();

            // Get a specific Todo item
            builder.MapGet("/api/items/{id}", async (
                [FromRoute] Guid id,
                ClaimsPrincipal user,
                ITodoService service,
                CancellationToken cancellationToken) =>
            {
                var model = await service.GetAsync(user.GetUserId(), id, cancellationToken);

                if (model is null)
                {
                    return new NotFoundResult();
                }

                return new ObjectResult(model) as IResult;
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
                    return new BadRequestResult();
                }

                var id = await service.AddItemAsync(user.GetUserId(), model.Text, cancellationToken);

                return new CreatedAtResult($"/api/items/{id}", new { id }) as IResult;
            }).RequireAuthorization();

            // Mark a Todo item as completed
            builder.MapPost("/api/items/{id}/complete", async (
                [FromRoute] Guid id,
                ClaimsPrincipal user,
                ITodoService service,
                CancellationToken cancellationToken) =>
            {
                var wasCompleted = await service.CompleteItemAsync(user.GetUserId(), id, cancellationToken);

                var statusCode = wasCompleted switch
                {
                    true => StatusCodes.Status204NoContent,
                    false => StatusCodes.Status400BadRequest,
                    _ => StatusCodes.Status404NotFound,
                };

                return new StatusCodeResult(statusCode);
            }).RequireAuthorization();

            // Delete a Todo item
            builder.MapDelete("/api/items/{id}", async (
                [FromRoute] Guid id,
                ClaimsPrincipal user,
                ITodoService service,
                CancellationToken cancellationToken) =>
            {
                var wasDeleted = await service.DeleteItemAsync(user.GetUserId(), id, cancellationToken);

                return new StatusCodeResult(wasDeleted ? StatusCodes.Status204NoContent : StatusCodes.Status404NotFound);
            }).RequireAuthorization();

            return builder;
        }

        // HACK Custom result types until ObjectResult implements IResult.
        // See https://github.com/dotnet/aspnetcore/issues/32565.

        private sealed class CreatedAtResult : ObjectResult
        {
            private readonly string _location;

            internal CreatedAtResult(string location, object? value)
                : base(value)
            {
                _location = location;
            }

            public override async Task ExecuteAsync(HttpContext httpContext)
            {
                httpContext.Response.Headers.Location = _location;
                await base.ExecuteAsync(httpContext);
            }
        }

        private class ObjectResult : IResult
        {
            public int? StatusCode { get; set; }

            public object? Value { get; set; }

            internal ObjectResult(object? value)
            {
                Value = value;
            }

            public virtual async Task ExecuteAsync(HttpContext httpContext)
            {
                var response = httpContext.Response;

                if (StatusCode.HasValue)
                {
                    response.StatusCode = StatusCode.Value;
                }

                await response.WriteAsJsonAsync(Value, httpContext.RequestAborted);
            }
        }
    }
}
