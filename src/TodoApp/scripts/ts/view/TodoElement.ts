// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

import moment from 'moment';
import { TodoItem } from '../models/TodoItem';
import { Classes } from './Classes';
import { Selectors } from './Selectors';

export class TodoElement {
    private readonly item: TodoItem;

    private readonly completeButton: Element;
    private readonly deleteButton: Element;
    private readonly itemElement: Element;
    private readonly timestampElement: Element;

    private onCompletedHandler: (id: string) => Promise<void>;
    private onDeletedHandler: (id: string) => void;
    private onDeletingHandler: (id: string) => Promise<void>;
    private textElement: Element;

    constructor(element: Element, item: TodoItem) {
        this.item = item;
        this.itemElement = element;

        this.itemElement.setAttribute('id', item.id);
        this.itemElement.setAttribute(
            'data-completed',
            item.isCompleted.toString()
        );
        this.itemElement.setAttribute('data-id', item.id);
        this.itemElement.setAttribute('data-timestamp', item.lastUpdated);

        this.completeButton = element.querySelector(Selectors.itemCompleted);
        this.deleteButton = element.querySelector(Selectors.deleteItem);
        this.textElement = element.querySelector(Selectors.itemText);
        this.timestampElement = element.querySelector(Selectors.itemTimestamp);

        this.textElement.textContent = item.text;

        if (item.isCompleted) {
            this.strikethrough();
        }

        this.updateTimestamp(moment(item.lastUpdated));

        if (!item.isCompleted) {
            this.completeButton.classList.remove(Classes.hidden);
            this.completeButton.addEventListener('click', () => {
                this.onCompleteItem();
            });
        }

        this.deleteButton.addEventListener('click', () => {
            this.onDeleteItem();
        });
    }

    id(): string {
        return this.item.id;
    }

    onComplete(handler: (id: string) => Promise<void>) {
        this.onCompletedHandler = handler;
    }

    onDeleting(handler: (id: string) => Promise<void>) {
        this.onDeletingHandler = handler;
    }

    onDeleted(handler: (id: string) => void) {
        this.onDeletedHandler = handler;
    }

    refresh() {
        this.updateTimestamp(moment(this.item.lastUpdated));
    }

    show() {
        this.itemElement.classList.remove(Classes.hidden);
    }

    private completed(timestamp: moment.Moment) {
        this.itemElement.setAttribute('data-completed', 'true');
        this.itemElement.setAttribute(
            'data-timestamp',
            timestamp.toISOString()
        );

        this.strikethrough();

        this.updateTimestamp(timestamp);
    }

    private async onCompleteItem(): Promise<void> {
        if (this.onCompletedHandler) {
            await this.onCompletedHandler(this.item.id);
        }

        this.completeButton.classList.add(Classes.hidden);

        const now = moment().milliseconds(0);
        this.completed(now);
    }

    private async onDeleteItem(): Promise<void> {
        if (this.onDeletingHandler) {
            await this.onDeletingHandler(this.item.id);
        }

        this.itemElement.remove();

        if (this.onDeletedHandler) {
            this.onDeletedHandler(this.item.id);
        }
    }

    private strikethrough() {
        let element = this.textElement;

        const text = element.textContent;
        element.textContent = '';

        const strikethrough = document.createElement('s');
        element.appendChild(strikethrough);
        element = strikethrough;
        element.textContent = text;

        this.textElement = strikethrough;
    }

    private updateTimestamp(timestamp: moment.Moment) {
        this.timestampElement.textContent = timestamp.fromNow();
        this.timestampElement.setAttribute('title', timestamp.toLocaleString());
    }
}
