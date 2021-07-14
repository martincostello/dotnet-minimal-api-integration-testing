// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace TodoApp
{
#pragma warning disable CA2252 // HACK Appears to be a bug - investigate in a later preview build
    [CollectionDefinition(Name)]
    public sealed class TodoAppCollection : ICollectionFixture<TodoAppFixture>
    {
        public const string Name = "TodoApp server collection";
    }
#pragma warning restore CA2252
}
