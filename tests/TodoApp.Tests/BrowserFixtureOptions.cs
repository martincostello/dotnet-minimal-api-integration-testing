// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace TodoApp;

public class BrowserFixtureOptions
{
    public string BrowserType { get; set; } = Microsoft.Playwright.BrowserType.Chromium;

    public string? BrowserChannel { get; set; }

    // Only record traces and videos in CI to prevent filling
    // up the local disk with videos from test runs.

    public bool CaptureTrace { get; set; } = BrowserFixture.IsRunningInGitHubActions;

    public bool CaptureVideo { get; set; } = BrowserFixture.IsRunningInGitHubActions;

    public string? TestName { get; set; }

    public string? Build { get; set; }

    public string? OperatingSystem { get; set; }

    public string? OperatingSystemVersion { get; set; }

    public string? PlaywrightVersion { get; set; }

    public string? ProjectName { get; set; }

    public bool UseBrowserStack { get; set; }

    public bool UseBrowserStackLocal { get; set; }

    public (string UserName, string AccessKey) BrowserStackCredentials { get; set; }

    public BrowserStackLocalOptions? BrowserStackLocalOptions { get; set; }

    public Uri BrowserStackEndpoint { get; set; } = new("wss://cdp.browserstack.com/playwright", UriKind.Absolute);
}
