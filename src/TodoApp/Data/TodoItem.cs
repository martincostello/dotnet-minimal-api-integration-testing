// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace TodoApp.Data;
public class TodoItem
{
    public Guid Id { get; set; }

    public string UserId { get; set; } = default!;

    public string Text { get; set; } = default!;

    public DateTime CreatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }
}
