// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.Playwright;

namespace TodoApp;

public class BrowserFixture
{
    private const string VideosDirectory = "videos";

    public BrowserFixture(
        BrowserFixtureOptions options,
        ITestOutputHelper outputHelper)
    {
        Options = options;
        OutputHelper = outputHelper;
    }

    // Only record videos in CI to prevent filling
    // up the local disk with videos from test runs.

    protected virtual bool RecordVideo { get; } = IsRunningInGitHubActions;

    private static bool IsRunningInGitHubActions { get; } = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTIONS"));

    private BrowserFixtureOptions Options { get; }

    private ITestOutputHelper OutputHelper { get; }

    public async Task WithPageAsync(
        Func<IPage, Task> action,
        [CallerMemberName] string? testName = null)
    {
        // Start Playright and load the browser of the specified type
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await CreateBrowserAsync(playwright);

        // Create a new page for the test to use
        var options = CreatePageOptions();
        var page = await browser.NewPageAsync(options);

        // Redirect the browser logs to the xunit output
        page.Console += (_, e) => OutputHelper.WriteLine(e.Text);
        page.PageError += (_, e) => OutputHelper.WriteLine(e);

        try
        {
            // Run the test, passing it the page to use
            await action(page);
        }
        catch (Exception)
        {
            // Try and capture a screenshot at the point the test failed
            await TryCaptureScreenshotAsync(page, Options.TestName ?? testName!);
            throw;
        }
        finally
        {
            await TryCaptureVideoAsync(page, Options.TestName ?? testName!);
        }
    }

    protected virtual BrowserNewPageOptions CreatePageOptions()
    {
        var options = new BrowserNewPageOptions
        {
            IgnoreHTTPSErrors = true, // The test fixture uses a self-signed TLS certificate
            Locale = "en-GB",
            TimezoneId = "Europe/London"
        };


        if (RecordVideo)
        {
            options.RecordVideoDir = VideosDirectory;
        }

        return options;
    }

    private async Task<IBrowser> CreateBrowserAsync(IPlaywright playwright)
    {
        var options = new BrowserTypeLaunchOptions
        {
            Channel = Options.BrowserChannel
        };

        // Slow down actions and make the DevTools visible by default
        // to make it easier to debug the app when debugging locally.
        if (System.Diagnostics.Debugger.IsAttached)
        {
            options.Devtools = true;
            options.Headless = false;
            options.SlowMo = 250;
        }

        return await playwright[Options.BrowserType].LaunchAsync(options);
    }

    private string GenerateFileName(string testName, string extension)
    {
        string browserType = Options.BrowserType;

        if (!string.IsNullOrEmpty(Options.BrowserChannel))
        {
            browserType += "_" + Options.BrowserChannel;
        }

        string os =
            OperatingSystem.IsLinux() ? "linux" :
            OperatingSystem.IsMacOS() ? "macos" :
            OperatingSystem.IsWindows() ? "windows" :
            "other";

        // Remove characters that are disallowed in file names
        browserType = browserType.Replace(':', '_');

        string utcNow = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture);
        return $"{testName}_{browserType}_{os}_{utcNow}{extension}";
    }

    private async Task TryCaptureScreenshotAsync(
        IPage page,
        string testName)
    {
        try
        {
            string fileName = GenerateFileName(testName, ".png");
            string path = Path.Combine("screenshots", fileName);

            await page.ScreenshotAsync(new() { Path = path });

            OutputHelper.WriteLine($"Screenshot saved to {path}.");
        }
        catch (Exception ex)
        {
            OutputHelper.WriteLine("Failed to capture screenshot: " + ex);
        }
    }

    private async Task TryCaptureVideoAsync(IPage page, string testName)
    {
        if (!RecordVideo || page.Video is null)
        {
            return;
        }

        try
        {
            string fileName = GenerateFileName(testName, ".webm");
            string path = Path.Combine(VideosDirectory, fileName);

            await page.CloseAsync();
            await page.Video.SaveAsAsync(path);

            OutputHelper.WriteLine($"Video saved to {path}.");
        }
        catch (Exception ex)
        {
            OutputHelper.WriteLine("Failed to capture video: " + ex);
        }
    }
}
