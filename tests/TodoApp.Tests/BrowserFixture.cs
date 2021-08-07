// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.Playwright;

namespace TodoApp;

public class BrowserFixture
{
    public BrowserFixture(ITestOutputHelper outputHelper)
    {
        OutputHelper = outputHelper;
    }

    // Only record videos in CI to prevent filling
    // up the local disk with videos from test runs.

    protected virtual bool RecordVideo { get; } = IsRunningInGitHubActions;

    private static bool IsRunningInGitHubActions { get; } = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTIONS"));

    private ITestOutputHelper OutputHelper { get; }

    public async Task WithPageAsync(
        Func<IPage, Task> action,
        string browserType = "chromium",
        [CallerMemberName] string? testName = null)
    {
        // Start Playright and load the browser of the specified type
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await CreateBrowserAsync(playwright, browserType);

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
            await TryCaptureScreenshotAsync(page, testName!, browserType);
            throw;
        }
        finally
        {
            await TryCaptureVideoAsync(page, testName!, browserType);
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
            options.RecordVideoDir = "videos";
        }

        return options;
    }

    private static async Task<IBrowser> CreateBrowserAsync(IPlaywright playwright, string browserType)
    {
        var options = new BrowserTypeLaunchOptions();

        // Slow down actions and make the DevTools visible by default
        // to make it easier to debug the app when debugging locally.
        if (System.Diagnostics.Debugger.IsAttached)
        {
            options.Devtools = true;
            options.Headless = false;
            options.SlowMo = 250;
        }

        // Configure a channel if one was specified, e.g. "chromium:chrome"
        var split = browserType.Split(':');

        browserType = split[0];

        if (split.Length > 1)
        {
            options.Channel = split[1];
        }

        return await playwright[browserType].LaunchAsync(options);
    }

    private static string GenerateFileName(string testName, string browserType, string extension)
    {
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
        string testName,
        string browserType)
    {
        try
        {
            string fileName = GenerateFileName(testName, browserType, ".png");
            string path = Path.Combine("screenshots", fileName);

            await page.ScreenshotAsync(new() { Path = path });

            OutputHelper.WriteLine($"Screenshot saved to {path}.");
        }
        catch (Exception ex)
        {
            OutputHelper.WriteLine("Failed to capture screenshot: " + ex);
        }
    }

    private async Task TryCaptureVideoAsync(
        IPage page,
        string testName,
        string browserType)
    {
        if (!RecordVideo)
        {
            return;
        }

        try
        {
            await page.CloseAsync();

            string videoSource = await page.Video!.PathAsync();

            string? directory = Path.GetDirectoryName(videoSource);
            string? extension = Path.GetExtension(videoSource);

            string fileName = GenerateFileName(testName, browserType, extension!);

            string videoDestination = Path.Combine(directory!, fileName);

            File.Move(videoSource, videoDestination);

            OutputHelper.WriteLine($"Video saved to {videoDestination}.");
        }
        catch (Exception ex)
        {
            OutputHelper.WriteLine("Failed to capture video: " + ex);
        }
    }
}
