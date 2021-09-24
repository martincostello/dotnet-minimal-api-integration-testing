// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace TodoApp;

public sealed class LoopbackOAuthEvents : OAuthEvents
{
    public override Task RedirectToAuthorizationEndpoint(RedirectContext<OAuthOptions> context)
    {
        // Extract the state and return URI from the intended
        // destination the browser would be directed to.
        var query = new UriBuilder(context.RedirectUri).Uri.Query;
        var queryString = HttpUtility.ParseQueryString(query);

        var location = queryString["redirect_uri"];
        var state = queryString["state"];

        queryString.Clear();

        // Redirect the browser back to the test application with
        // the state from the original intended destination.
        var code = Guid.NewGuid().ToString();

        queryString.Add("code", code);
        queryString.Add("state", state);

        var builder = new UriBuilder(location!)
        {
            Query = queryString.ToString() ?? string.Empty
        };

        context.RedirectUri = builder.ToString();

        return base.RedirectToAuthorizationEndpoint(context);
    }
}
