// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace TodoApp.Data;
public sealed class TodoRepository : ITodoRepository
{
    public TodoRepository(IClock clock, TodoContext context)
    {
        Clock = clock;
        Context = context;
    }

    private IClock Clock { get; }

    private TodoContext Context { get; }

    public async Task<TodoItem> AddItemAsync(
        string userId,
        string text,
        CancellationToken cancellationToken = default)
    {
        await EnsureDatabaseAsync(cancellationToken);

        var item = new TodoItem()
        {
            CreatedAt = Now(),
            Text = text,
            UserId = userId,
        };

        Context.Add(item);

        await Context.SaveChangesAsync(cancellationToken);

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

        item.CompletedAt = Now();

        Context.Items.Update(item);

        await Context.SaveChangesAsync(cancellationToken);

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

        Context.Items.Remove(item);

        await Context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<TodoItem?> GetItemAsync(
        string userId,
        Guid itemId,
        CancellationToken cancellationToken = default)
    {
        await EnsureDatabaseAsync(cancellationToken);

        var item = await Context.Items.FindItemAsync(itemId, cancellationToken);

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

        return await Context.Items
            .Where((p) => p.UserId == userId)
            .OrderBy((p) => p.CompletedAt.HasValue)
            .ThenBy((p) => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    private async Task EnsureDatabaseAsync(CancellationToken cancellationToken)
        => await Context.Database.EnsureCreatedAsync(cancellationToken);

    private DateTime Now() => Clock.GetCurrentInstant().ToDateTimeUtc();
}
