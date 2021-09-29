// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Security.Claims;
using AspNet.Security.OAuth.GitHub;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace TodoApp;

/// <summary>
/// A class containing the HTTP endpoints and extension methods for authentication.
/// </summary>
public static class AuthenticationEndpoints
{
    private const string DeniedPath = "/denied";
    private const string RootPath = "/";
    private const string SignInPath = "/sign-in";
    private const string SignOutPath = "/sign-out";

    private const string GitHubAvatarClaim = "urn:github:avatar";
    private const string GitHubProfileClaim = "urn:github:profile";

    /// <summary>
    /// Adds GitHub authentication to the application.
    /// </summary>
    /// <param name="builder">The <see cref="IServiceCollection"/>.</param>
    /// <returns>
    /// A <see cref="IServiceCollection"/> that can be used to further configure the application.
    /// </returns>
    public static IServiceCollection AddGitHubAuthentication(this IServiceCollection builder)
    {
        return builder
            .AddAuthentication(options => options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = SignInPath;
                options.LogoutPath = SignOutPath;
            })
            .AddGitHub()
            .Services
            .AddOptions<GitHubAuthenticationOptions>(GitHubAuthenticationDefaults.AuthenticationScheme)
            .Configure<IConfiguration>((options, configuration) =>
            {
                options.AccessDeniedPath = DeniedPath;
                options.CallbackPath = SignInPath + "-github";
                options.ClientId = configuration["GitHub:ClientId"];
                options.ClientSecret = configuration["GitHub:ClientSecret"];
                options.EnterpriseDomain = configuration["GitHub:EnterpriseDomain"];

                options.Scope.Add("user:email");
                options.ClaimActions.MapJsonKey(GitHubProfileClaim, "html_url");

                if (string.IsNullOrEmpty(options.EnterpriseDomain))
                {
                    options.ClaimActions.MapJsonKey(GitHubAvatarClaim, "avatar_url");
                }
            })
            .ValidateOnStart()
            .Services;
    }

    /// <summary>
    /// Gets the user's GitHub avatar URL.
    /// </summary>
    /// <param name="user">The current user.</param>
    /// <returns>
    /// The GitHub avatar URL for the current user, if any.
    /// </returns>
    public static string? GetAvatarUrl(this ClaimsPrincipal user)
        => user.FindFirst(GitHubAvatarClaim)?.Value;

    /// <summary>
    /// Gets the user's GitHub profile URL.
    /// </summary>
    /// <param name="user">The current user.</param>
    /// <returns>
    /// The GitHub profile URL for the current user.
    /// </returns>
    public static string GetProfileUrl(this ClaimsPrincipal user)
        => user.FindFirst(GitHubProfileClaim)!.Value;

    /// <summary>
    /// Gets the user's user Id.
    /// </summary>
    /// <param name="user">The current user.</param>
    /// <returns>
    /// The user Id for the current user.
    /// </returns>
    public static string GetUserId(this ClaimsPrincipal user)
        => user.FindFirst(ClaimTypes.NameIdentifier)!.Value;

    /// <summary>
    /// Gets the user's user name.
    /// </summary>
    /// <param name="user">The current user.</param>
    /// <returns>
    /// The user name for the current user.
    /// </returns>
    public static string GetUserName(this ClaimsPrincipal user)
        => user.FindFirst(GitHubAuthenticationConstants.Claims.Name)!.Value;

    /// <summary>
    /// Maps the endpoints for authentication.
    /// </summary>
    /// <param name="builder">The <see cref="IEndpointConventionBuilder"/>.</param>
    /// <returns>
    /// A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.
    /// </returns>
    public static IEndpointRouteBuilder MapAuthenticationRoutes(this IEndpointRouteBuilder builder)
    {
        builder.MapGet(DeniedPath, () => Results.Redirect(RootPath + "?denied=true"))
               .ExcludeFromDescription();

        builder.MapGet(SignOutPath, () => Results.Redirect(RootPath))
               .ExcludeFromDescription();

        builder.MapPost(SignInPath, () => Results.Challenge(new() { RedirectUri = RootPath }, new[] { GitHubAuthenticationDefaults.AuthenticationScheme }))
               .ExcludeFromDescription();

        builder.MapPost(SignOutPath, () => Results.SignOut(new() { RedirectUri = RootPath }, new[] { CookieAuthenticationDefaults.AuthenticationScheme }))
               .ExcludeFromDescription()
               .RequireAuthorization();

        return builder;
    }
}
