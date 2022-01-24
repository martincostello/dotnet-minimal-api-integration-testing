// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace TodoApp;

public class BrowserFixtureOptions
{
    public string BrowserType { get; set; } = Microsoft.Playwright.BrowserType.Chromium;

    public string? BrowserChannel { get; set; }

    public string? TestName { get; set; }
}
