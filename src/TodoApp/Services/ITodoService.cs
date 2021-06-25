// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using TodoApp.Models;

namespace TodoApp.Services
{
    public interface ITodoService
    {
        Task<string> AddItemAsync(string userId, string text, CancellationToken cancellationToken);

        Task<bool?> CompleteItemAsync(string userId, string itemId, CancellationToken cancellationToken);

        Task<bool> DeleteItemAsync(string userId, string itemId, CancellationToken cancellationToken);

        Task<TodoItemModel?> GetAsync(string userId, string itemId, CancellationToken cancellationToken);

        Task<TodoListViewModel> GetListAsync(string userId, CancellationToken cancellationToken);
    }
}
