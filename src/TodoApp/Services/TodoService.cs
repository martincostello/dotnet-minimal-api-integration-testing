// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using TodoApp.Data;
using TodoApp.Models;

namespace TodoApp.Services;

public sealed class TodoService(ITodoRepository repository) : ITodoService
{
    public async Task<string> AddItemAsync(
        string userId,
        string text,
        CancellationToken cancellationToken)
    {
        var item = await repository.AddItemAsync(userId, text, cancellationToken);

        return item.Id.ToString();
    }

    public async Task<bool?> CompleteItemAsync(
        string userId,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        return await repository.CompleteItemAsync(userId, itemId, cancellationToken);
    }

    public async Task<bool> DeleteItemAsync(
        string userId,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        return await repository.DeleteItemAsync(userId, itemId, cancellationToken);
    }

    public async Task<TodoItemModel?> GetAsync(
        string userId,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        var item = await repository.GetItemAsync(userId, itemId, cancellationToken);

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
            var items = await repository.GetItemsAsync(userId, cancellationToken);

            foreach (var todo in items)
            {
                result.Items.Add(MapItem(todo));
            }
        }

        return result;
    }

    private static TodoItemModel MapItem(TodoItem item)
    {
        return new TodoItemModel
        {
            Id = item.Id.ToString(),
            IsCompleted = item.CompletedAt.HasValue,
            LastUpdated = (item.CompletedAt ?? item.CreatedAt).ToString("u", CultureInfo.InvariantCulture),
            Text = item.Text
        };
    }
}
