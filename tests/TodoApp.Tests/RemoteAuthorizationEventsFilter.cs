// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Collections.Specialized;
using System.Web;
using AspNet.Security.OAuth.GitHub;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace TodoApp;

public sealed class RemoteAuthorizationEventsFilter : IPostConfigureOptions<GitHubAuthenticationOptions>
{
    public RemoteAuthorizationEventsFilter(IHttpClientFactory httpClientFactory)
    {
        HttpClientFactory = httpClientFactory;
    }

    private IHttpClientFactory HttpClientFactory { get; }

    public void PostConfigure(string name, GitHubAuthenticationOptions options)
    {
        // Use HttpClientFactory for HTTP requests so that the tests
        // can intercept the request and return canned responses.
        options.Backchannel = HttpClientFactory.CreateClient(name);

        // Configure the GitHub provider to redirect back to the
        // test application, rather than GitHub's own login pages.
        options.Events.OnRedirectToAuthorizationEndpoint = RedirectToSelfForOAuth;
    }

    private static Task RedirectToSelfForOAuth<T>(RedirectContext<T> context)
        where T : AuthenticationSchemeOptions
    {
        // Extract the state and return URI from the intended
        // destination the browser would be directed to.
        NameValueCollection queryString = ParseQueryString(context);

        string? location = queryString["redirect_uri"];
        string? state = queryString["state"];

        queryString.Clear();

        // Redirect the browser back to the test application with
        // the state from the original intended destination.
        string code = Guid.NewGuid().ToString();

        queryString.Add("code", code);
        queryString.Add("state", state);

        var builder = new UriBuilder(location!)
        {
            Query = queryString.ToString() ?? string.Empty,
        };

        return Redirect(context, builder);
    }

    private static NameValueCollection ParseQueryString<T>(RedirectContext<T> context)
        where T : AuthenticationSchemeOptions
    {
        return HttpUtility.ParseQueryString(new UriBuilder(context.RedirectUri).Uri.Query);
    }

    private static Task Redirect<T>(RedirectContext<T> context, UriBuilder builder)
        where T : AuthenticationSchemeOptions
    {
        string location = builder.Uri.ToString();

        context.Response.Redirect(location);

        return Task.CompletedTask;
    }
}
