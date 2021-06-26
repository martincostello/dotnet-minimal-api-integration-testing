// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

import { TodoClient } from '../client/TodoClient';
import { TodoItem } from '../models/TodoItem';
import { Classes } from './Classes';
import { Elements } from './Elements';

export class TodoApp {

    private readonly client: TodoClient;
    private readonly elements: Elements;

    constructor() {
        this.client = new TodoClient();
        this.elements = new Elements();
    }

    async initialize(): Promise<void> {

        // Return if the app is not signed in
        if (!this.elements.createItemForm) {
            return;
        }

        // Disable the default HTML form
        this.elements.createItemForm.addEventListener('submit', (event) => {
            event.preventDefault();
            return false;
        });

        // Disable/enable the add button when text is absent/present
        this.elements.createItemText.addEventListener('input', () => {
            if (this.elements.createItemText.value.length === 0) {
                this.disable(this.elements.createItemButton);
            } else {
                this.enable(this.elements.createItemButton);
            }
        });

        // Add a new Todo item when the button is clicked
        this.elements.createItemButton.addEventListener('click', () => {
            this.addNewItem();
        });

        // Load and render the existing Todo items
        const items = await this.client.getAll();

        items.forEach((item) => {
            this.createItem(item);
        });

        // Initialize the UI elements
        if (items.length > 0) {
            this.show(this.elements.itemTable);
        } else {
            this.show(this.elements.banner);
        }

        this.hide(this.elements.loader);
    }

    async addNewItem(): Promise<void> {

        this.disable(this.elements.createItemButton);
        this.disable(this.elements.createItemText);

        try {

            const text = this.elements.createItemText.value;

            // Add the new Todo item and then fetch it
            const id = await this.client.add(text);
            const item = await this.client.get(id);

            // Render the item
            this.createItem(item);

            // Reset the UI for adding another item
            this.elements.createItemText.value = '';
            this.hide(this.elements.banner);
            this.show(this.elements.itemTable);

        } catch {
            // Re-enable adding this item if it failed
            this.enable(this.elements.createItemButton);
        } finally {
            this.enable(this.elements.createItemText);
            this.elements.createItemText.focus();
        }
    }

    createItem(item: TodoItem) {

        const element = this.elements.createNewItem(item);

        // Wire-up event handler to complete the Todo item if required
        if (!item.isCompleted) {
            element.onComplete(async (id) => {
                await this.client.complete(id);
            });
        }

        // Wire-up event handler to delete the Todo item
        element.onDeleting(async (id) => {
            await this.client.delete(id);
        });
        element.onDeleted(() => {
            if (this.elements.itemCount() < 1) {
                this.hide(this.elements.itemTable);
                this.show(this.elements.banner);
            }
        });

        element.show();
    }

    private disable(element: Element) {
        element.setAttribute('disabled', '');
    }

    private enable(element: Element) {
        element.removeAttribute('disabled');
    }

    private hide(element: Element) {
        element.classList.add(Classes.hidden);
    }

    private show(element: Element) {
        element.classList.remove(Classes.hidden);
    }
}
