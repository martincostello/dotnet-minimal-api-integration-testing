// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

import { TodoApp } from './view/TodoApp';

document.addEventListener('DOMContentLoaded', () => {
    const app = new TodoApp();
    app.initialize();
});
