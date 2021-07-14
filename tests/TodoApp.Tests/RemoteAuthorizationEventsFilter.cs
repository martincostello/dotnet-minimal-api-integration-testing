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
        options.Backchannel = HttpClientFactory.CreateClient(name);
        options.Events.OnRedirectToAuthorizationEndpoint = RedirectToSelfForOAuth;
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

    private static Task RedirectToSelfForOAuth<T>(RedirectContext<T> context)
        where T : AuthenticationSchemeOptions
    {
        NameValueCollection queryString = ParseQueryString(context);

        string? location = queryString["redirect_uri"];
        string? state = queryString["state"];

        queryString.Clear();

        string code = Guid.NewGuid().ToString();

        queryString.Add("code", code);
        queryString.Add("state", state);

        var builder = new UriBuilder(location!)
        {
            Query = queryString.ToString() ?? string.Empty,
        };

        return Redirect(context, builder);
    }
}
