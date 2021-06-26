// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Security.Claims;
using AspNet.Security.OAuth.GitHub;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TodoApp
{
    public static class AuthenticationModule
    {
        private const string DeniedPath = "/denied";
        private const string RootPath = "/";
        private const string SignInPath = "/signin";
        private const string SignOutPath = "/signout";

        private const string GitHubAvatarClaim = "urn:github:avatar";
        private const string GitHubProfileClaim = "urn:github:profile";

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
                .Configure<IConfiguration, IHostEnvironment>((options, configuration, environment) =>
                {
                    options.AccessDeniedPath = DeniedPath;
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
                .Services;
        }

        public static string? GetAvatarUrl(this ClaimsPrincipal user)
            => user.FindFirst(GitHubAvatarClaim)?.Value;

        public static string GetProfileUrl(this ClaimsPrincipal user)
            => user.FindFirst(GitHubProfileClaim)!.Value;

        public static string GetUserId(this ClaimsPrincipal user)
            => user.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        public static string GetUserName(this ClaimsPrincipal user)
            => user.FindFirst(GitHubAuthenticationConstants.Claims.Name)!.Value;

        public static IEndpointRouteBuilder MapAuthenticationRoutes(this IEndpointRouteBuilder builder)
        {
            // TODO Remove cast once supported by C# 10
            builder.MapGet(DeniedPath, (Action<HttpContext>)(context => context.Response.Redirect(RootPath + "?denied=true")));
            builder.MapGet(SignOutPath, (Action<HttpContext>)(context => context.Response.Redirect(RootPath)));

            builder.MapPost(SignInPath, async context =>
            {
                await context.ChallengeAsync(
                    GitHubAuthenticationDefaults.AuthenticationScheme,
                    new AuthenticationProperties { RedirectUri = RootPath });
            });

            builder.MapPost(SignOutPath, async context =>
            {
                await context.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new AuthenticationProperties { RedirectUri = RootPath });
            }).RequireAuthorization();

            return builder;
        }
    }
}
