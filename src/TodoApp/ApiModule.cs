// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using TodoApp.Models;
using TodoApp.Services;

namespace TodoApp
{
    public static class ApiModule
    {
        private const string ItemCompletePath = "/api/items/{id}/complete";
        private const string ItemPath = "/api/items/{id}";
        private const string ItemsPath = "/api/items";

        public static IEndpointRouteBuilder MapApiRoutes(this IEndpointRouteBuilder builder)
        {
            builder.MapPost(ItemsPath, async (context) =>
            {
                var model = await context.Request.ReadFromJsonAsync<CreateTodoItemModel>();

                if (model is null || string.IsNullOrWhiteSpace(model.Text))
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    return;
                }

                var userId = context.GetUserId();
                var service = context.RequestServices.GetRequiredService<ITodoService>();

                var id = await service.AddItemAsync(userId, model.Text, context.RequestAborted);

                context.Response.StatusCode = StatusCodes.Status201Created;
                context.Response.GetTypedHeaders().Location = new Uri($"/api/items/{id}", UriKind.Relative);

                await context.Response.WriteAsJsonAsync(new { id });
            }).RequireAuthorization();

            builder.MapGet(ItemsPath, async (context) =>
            {
                var userId = context.GetUserId();
                var service = context.GetTodoService();

                var model = await service.GetListAsync(userId, context.RequestAborted);

                await context.Response.WriteAsJsonAsync(model);
            }).RequireAuthorization();

            builder.MapGet(ItemPath, async (context) =>
            {
                var itemId = context.GetItemId();
                var userId = context.GetUserId();
                var service = context.GetTodoService();

                var model = await service.GetAsync(userId, itemId, context.RequestAborted);

                if (model is null)
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }

                await context.Response.WriteAsJsonAsync(model);
            }).RequireAuthorization();

            builder.MapPost(ItemCompletePath, async (context) =>
            {
                var itemId = context.GetItemId();
                var userId = context.GetUserId();
                var service = context.GetTodoService();

                var result = await service.CompleteItemAsync(userId, itemId, context.RequestAborted);

                context.Response.StatusCode = result switch
                {
                    true => StatusCodes.Status204NoContent,
                    false => StatusCodes.Status400BadRequest,
                    _ => StatusCodes.Status404NotFound,
                };
            }).RequireAuthorization();

            builder.MapDelete(ItemPath, async (context) =>
            {
                var itemId = context.GetItemId();
                var userId = context.GetUserId();
                var service = context.GetTodoService();

                var result = await service.DeleteItemAsync(userId, itemId, context.RequestAborted);

                context.Response.StatusCode = result ? StatusCodes.Status204NoContent : StatusCodes.Status404NotFound;
            }).RequireAuthorization();

            return builder;
        }

        private static string GetItemId(this HttpContext context)
            => (string)context.GetRouteValue("id")!;

        private static ITodoService GetTodoService(this HttpContext context)
            => context.RequestServices.GetRequiredService<ITodoService>();
    }
}
