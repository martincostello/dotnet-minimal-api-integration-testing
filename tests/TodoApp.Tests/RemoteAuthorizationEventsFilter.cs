// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using AspNet.Security.OAuth.GitHub;
using Microsoft.Extensions.Options;

namespace TodoApp;

public sealed class RemoteAuthorizationEventsFilter(IHttpClientFactory httpClientFactory) : IPostConfigureOptions<GitHubAuthenticationOptions>
{
    public void PostConfigure(string? name, GitHubAuthenticationOptions options)
    {
        // Use HttpClientFactory for HTTP requests so that the tests
        // can intercept the request and return canned responses.
        options.Backchannel = httpClientFactory.CreateClient(name ?? string.Empty);

        // Configure the GitHub provider to redirect back to the
        // test application, rather than GitHub's own login pages.
        options.EventsType = typeof(LoopbackOAuthEvents);
    }
}
