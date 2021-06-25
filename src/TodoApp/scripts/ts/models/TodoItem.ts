// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

export interface TodoItem {
    id: string;
    text: string;
    isCompleted: boolean;
    lastUpdated: string;
}
