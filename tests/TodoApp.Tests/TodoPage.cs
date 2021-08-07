// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.Playwright;

namespace TodoApp;

/// <summary>
/// A class represeting the Page Object Model for the Todo application.
/// </summary>
public class TodoPage
{
    public TodoPage(IPage page)
    {
        Page = page;
    }

    private IPage Page { get; }

    public async Task AddItemAsync(string text)
    {
        await Page.TypeAsync(Selectors.AddItemText, text);
        await Page.ClickAsync(Selectors.AddItemButton);

        var input = await Page.QuerySelectorAsync(Selectors.AddItemText);
        await input!.WaitForElementStateAsync(ElementState.Editable);
    }

    public async Task<IReadOnlyList<TodoPageItem>> GetItemsAsync()
    {
        var elements = await Page.QuerySelectorAllAsync(Selectors.TodoItem);
        return elements.Select(x => new TodoPageItem(x)).ToArray();
    }

    public async Task SignInAsync()
        => await Page.ClickAsync(Selectors.SignIn);

    public async Task SignOutAsync()
        => await Page.ClickAsync(Selectors.SignOut);

    public async Task<string> UserNameAsync()
        => await Page.InnerTextAsync(Selectors.UserName);

    public async Task WaitForNoItemsAsync()
        => await Page.WaitForSelectorAsync(Selectors.NoItems);

    public async Task WaitForSignedInAsync()
        => await Page.WaitForSelectorAsync(Selectors.UserName);

    public async Task WaitForSignedOutAsync()
        => await Page.WaitForSelectorAsync(Selectors.SignIn);

    public sealed class TodoPageItem
    {
        internal TodoPageItem(IElementHandle item)
        {
            Item = item;
        }

        private IElementHandle Item { get; }

        public async Task CompleteAsync()
        {
            var element = await Item.QuerySelectorAsync(Selectors.CompleteItem);
            await element!.ClickAsync();
        }

        public async Task DeleteAsync()
        {
            var element = await Item.QuerySelectorAsync(Selectors.DeleteItem);
            await element!.ClickAsync();
        }

        public async Task<string> TextAsync()
        {
            var element = await Item.QuerySelectorAsync(Selectors.ItemText);
            return await element!.InnerTextAsync();
        }

        public async Task<string> LastUpdatedAsync()
        {
            var element = await Item.QuerySelectorAsync(Selectors.ItemTimestamp);
            return await element!.InnerTextAsync();
        }
    }

    private sealed class Selectors
    {
        internal const string AddItemButton = "id=add-new-item";
        internal const string AddItemText = "id=new-item-text";
        internal const string CompleteItem = "button[class*='todo-item-complete']";
        internal const string DeleteItem = "button[class*='todo-item-delete']";
        internal const string ItemText = "[class*='todo-item-text']";
        internal const string ItemTimestamp = "[class*='todo-item-timestamp']";
        internal const string NoItems = "id=banner";
        internal const string SignIn = "id=sign-in";
        internal const string SignOut = "id=sign-out";
        internal const string TodoItem = "[class='todo-item']";
        internal const string UserName = "id=user-name";
    }
}
