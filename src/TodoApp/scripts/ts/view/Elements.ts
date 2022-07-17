// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

import { TodoItem } from '../models/TodoItem';
import { Classes } from './Classes';
import { Selectors } from './Selectors';
import { TodoElement } from './TodoElement';

export class Elements {
    readonly banner: HTMLElement;
    readonly createItemButton: HTMLElement;
    readonly createItemForm: HTMLElement;
    readonly createItemText: HTMLInputElement;
    readonly itemList: HTMLElement;
    readonly itemTable: HTMLElement;
    readonly itemTemplate: HTMLElement;
    readonly loader: HTMLElement;

    constructor() {
        this.banner = document.getElementById('banner');
        this.createItemButton = document.getElementById('add-new-item');
        this.createItemForm = document.getElementById('add-form');
        this.createItemText = <HTMLInputElement>(
            document.getElementById('new-item-text')
        );
        this.itemList = document.getElementById('item-list');
        this.itemTable = document.getElementById('item-table');
        this.itemTemplate = document.getElementById('item-template');
        this.loader = document.getElementById('loader');
    }

    createNewItem(item: TodoItem): TodoElement {
        // Clone the template and add to the table
        const node = this.itemTemplate.cloneNode(true);
        this.itemList.appendChild(node);

        // Turn the template into a new item
        const element = this.itemList.lastElementChild;
        element.classList.add(Classes.item);

        return new TodoElement(element, item);
    }

    itemCount(): number {
        return this.itemList.querySelectorAll(Selectors.item).length;
    }
}
