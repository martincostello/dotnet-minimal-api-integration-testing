// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace TodoApp.Data;

public interface ITodoRepository
{
    Task<TodoItem> AddItemAsync(string userId, string text, CancellationToken cancellationToken = default);

    Task<bool?> CompleteItemAsync(string userId, Guid itemId, CancellationToken cancellationToken = default);

    Task<bool> DeleteItemAsync(string userId, Guid itemId, CancellationToken cancellationToken = default);

    Task<TodoItem?> GetItemAsync(string userId, Guid itemId, CancellationToken cancellationToken = default);

    Task<IList<TodoItem>> GetItemsAsync(string userId, CancellationToken cancellationToken = default);
}
