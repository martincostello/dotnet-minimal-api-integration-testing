// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.EntityFrameworkCore;

namespace TodoApp.Data;

public sealed class TodoRepository(TimeProvider timeProvider, TodoContext context) : ITodoRepository
{
    public async Task<TodoItem> AddItemAsync(
        string userId,
        string text,
        CancellationToken cancellationToken = default)
    {
        await EnsureDatabaseAsync(cancellationToken);

        var item = new TodoItem
        {
            CreatedAt = UtcNow(),
            Text = text,
            UserId = userId
        };

        context.Add(item);

        await context.SaveChangesAsync(cancellationToken);

        return item;
    }

    public async Task<bool?> CompleteItemAsync(
        string userId,
        Guid itemId,
        CancellationToken cancellationToken = default)
    {
        var item = await GetItemAsync(userId, itemId, cancellationToken);

        if (item is null)
        {
            return null;
        }

        if (item.CompletedAt.HasValue)
        {
            return false;
        }

        item.CompletedAt = UtcNow();

        context.Items.Update(item);

        await context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteItemAsync(
        string userId,
        Guid itemId,
        CancellationToken cancellationToken = default)
    {
        var item = await GetItemAsync(userId, itemId, cancellationToken);

        if (item is null)
        {
            return false;
        }

        context.Items.Remove(item);

        await context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<TodoItem?> GetItemAsync(
        string userId,
        Guid itemId,
        CancellationToken cancellationToken = default)
    {
        await EnsureDatabaseAsync(cancellationToken);

        var item = await context.Items.FindItemAsync(itemId, cancellationToken);

        if (item is null || !string.Equals(item.UserId, userId, StringComparison.Ordinal))
        {
            return null;
        }

        return item;
    }

    public async Task<IList<TodoItem>> GetItemsAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        await EnsureDatabaseAsync(cancellationToken);

        return await context.Items
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.CompletedAt.HasValue)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    private async Task EnsureDatabaseAsync(CancellationToken cancellationToken)
        => await context.Database.EnsureCreatedAsync(cancellationToken);

    private DateTime UtcNow() => timeProvider.GetUtcNow().UtcDateTime;
}
