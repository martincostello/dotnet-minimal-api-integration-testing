// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.Playwright;

namespace TodoApp;

[Collection(HttpServerCollection.Name)]
public class UITests : IAsyncLifetime
{
    public UITests(HttpServerFixture fixture, ITestOutputHelper outputHelper)
    {
        Fixture = fixture;

        // Route output from the fixture's logs to xunit's output
        OutputHelper = outputHelper;
        Fixture.SetOutputHelper(OutputHelper);
    }

    private HttpServerFixture Fixture { get; }

    private ITestOutputHelper OutputHelper { get; }

    public static TheoryData<string, string?> Browsers()
    {
        var browsers = new TheoryData<string, string?>()
        {
            { BrowserType.Chromium, null },
            { BrowserType.Chromium, "chrome" },
            { BrowserType.Firefox, null },
        };

        // HACK Skip on macOS. See https://github.com/microsoft/playwright-dotnet/issues/2920.
        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
        {
            browsers.Add(BrowserType.Chromium, "msedge");
        }

        if (OperatingSystem.IsMacOS())
        {
            browsers.Add(BrowserType.Webkit, null);
        }

        return browsers;
    }

    [Theory]
    [MemberData(nameof(Browsers))]
    public async Task Can_Sign_In_And_Manage_Todo_Items(string browserType, string? browserChannel)
    {
        // Arrange
        var options = new BrowserFixtureOptions
        {
            BrowserType = browserType,
            BrowserChannel = browserChannel
        };

        var browser = new BrowserFixture(options, OutputHelper);
        await browser.WithPageAsync(async page =>
        {
            // Load the application
            await page.GotoAsync(Fixture.ServerAddress);
            await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

            var app = new TodoPage(page);

            // Act - Sign in
            await app.SignInAsync();

            // Assert
            await app.WaitForSignedInAsync();
            await app.UserNameAsync().ShouldBe("John Smith");

            // Arrange - Wait for list to be ready
            await app.WaitForNoItemsAsync();

            // Act - Add an item
            await app.AddItemAsync("Buy cheese");

            // Assert
            var items = await app.GetItemsAsync();
            items.Count.ShouldBe(1);

            await items[0].TextAsync().ShouldBe("Buy cheese");
            await items[0].LastUpdatedAsync().ShouldBe("a few seconds ago");

            // Act - Add another item
            await app.AddItemAsync("Buy eggs");

            // Assert
            items = await app.GetItemsAsync();
            items.Count.ShouldBe(2);

            await items[0].TextAsync().ShouldBe("Buy cheese");
            await items[1].TextAsync().ShouldBe("Buy eggs");

            // Act - Delete an item and complete an item
            await items[0].DeleteAsync();
            await items[1].CompleteAsync();

            await Task.Delay(TimeSpan.FromSeconds(0.5));

            // Assert
            items = await app.GetItemsAsync();
            items.Count.ShouldBe(1);

            await items[0].TextAsync().ShouldBe("Buy eggs");

            // Act - Delete the remaining item
            await items[0].DeleteAsync();

            // Assert
            await app.WaitForNoItemsAsync();

            // Act - Sign out
            await app.SignOutAsync();

            // Assert
            await app.WaitForSignedOutAsync();
        });
    }

    public Task InitializeAsync()
    {
        InstallPlaywright();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private static void InstallPlaywright()
    {
        int exitCode = Microsoft.Playwright.Program.Main(["install"]);

        if (exitCode != 0)
        {
            throw new InvalidOperationException($"Playwright exited with code {exitCode}");
        }
    }
}
