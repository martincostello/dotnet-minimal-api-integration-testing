// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using TodoApp.Data;
using TodoApp.Models;

namespace TodoApp.Services;

public sealed class TodoService : ITodoService
{
    public TodoService(ITodoRepository repository)
    {
        Repository = repository;
    }

    private ITodoRepository Repository { get; }

    public async Task<string> AddItemAsync(
        string userId,
        string text,
        CancellationToken cancellationToken)
    {
        var item = await Repository.AddItemAsync(userId, text, cancellationToken);

        return item.Id.ToString();
    }

    public async Task<bool?> CompleteItemAsync(
        string userId,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        return await Repository.CompleteItemAsync(userId, itemId, cancellationToken);
    }

    public async Task<bool> DeleteItemAsync(
        string userId,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        return await Repository.DeleteItemAsync(userId, itemId, cancellationToken);
    }

    public async Task<TodoItemModel?> GetAsync(
        string userId,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        var item = await Repository.GetItemAsync(userId, itemId, cancellationToken);

        if (item is null)
        {
            return null;
        }

        return MapItem(item);
    }

    public async Task<TodoListViewModel> GetListAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        var result = new TodoListViewModel();

        if (!string.IsNullOrEmpty(userId))
        {
            var items = await Repository.GetItemsAsync(userId, cancellationToken);

            foreach (var todo in items)
            {
                result.Items.Add(MapItem(todo));
            }
        }

        return result;
    }

    private static TodoItemModel MapItem(TodoItem item)
    {
        return new TodoItemModel()
        {
            Id = item.Id.ToString(),
            IsCompleted = item.CompletedAt.HasValue,
            LastUpdated = (item.CompletedAt ?? item.CreatedAt).ToString("u", CultureInfo.InvariantCulture),
            Text = item.Text,
        };
    }
}
