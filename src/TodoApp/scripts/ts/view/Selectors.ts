// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

import { Classes } from './Classes';

export class Selectors {
    static deleteItem = '.todo-item-delete';
    static itemCompleted = '.todo-item-complete';
    static item = '.' + Classes.item;
    static itemText = '.todo-item-text';
    static itemTimestamp = '.todo-item-timestamp';
}
