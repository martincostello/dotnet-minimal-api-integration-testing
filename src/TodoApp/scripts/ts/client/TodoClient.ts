// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

import { TodoItem } from '../models/TodoItem';
import { TodoList } from '../models/TodoList';

export class TodoClient {
    async add(text: string): Promise<string> {
        const payload = {
            text: text,
        };

        const headers = new Headers();
        headers.set('Accept', 'application/json');
        headers.set('Content-Type', 'application/json');

        const init = {
            method: 'POST',
            headers: headers,
            body: JSON.stringify(payload),
        };

        const response = await fetch('/api/items', init);

        if (!response.ok) {
            throw new Error(response.status.toString(10));
        }

        const result = await response.json();
        return result.id;
    }

    async complete(id: string): Promise<void> {
        const init = {
            method: 'POST',
        };

        const url = `/api/items/${encodeURIComponent(id)}/complete`;

        const response = await fetch(url, init);

        if (!response.ok) {
            throw new Error(response.status.toString(10));
        }
    }

    async delete(id: string): Promise<void> {
        const init = {
            method: 'DELETE',
        };

        const url = `/api/items/${encodeURIComponent(id)}`;

        const response = await fetch(url, init);

        if (!response.ok) {
            throw new Error(response.status.toString(10));
        }
    }

    async get(id: string): Promise<TodoItem> {
        const response = await fetch(`/api/items/${encodeURIComponent(id)}`);

        if (!response.ok) {
            throw new Error(response.status.toString(10));
        }

        return await response.json();
    }

    async getAll(): Promise<TodoItem[]> {
        const response = await fetch('/api/items');

        if (!response.ok) {
            throw new Error(response.status.toString(10));
        }

        const result: TodoList = await response.json();
        return result.items;
    }
}
