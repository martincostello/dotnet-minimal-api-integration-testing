// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using AspNet.Security.OAuth.GitHub;
using JustEat.HttpClientInterception;
using MartinCostello.Logging.XUnit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TodoApp;

public class TodoAppFixture : WebApplicationFactory<Program>, ITestOutputHelperAccessor
{
    public TodoAppFixture()
    {
        // Use HTTPS by default and do not follow
        // redirects so they can tested explicitly.
        ClientOptions.AllowAutoRedirect = false;
        ClientOptions.BaseAddress = new Uri("https://localhost");

        // Configure HTTP requests that are not intercepted by
        // the tests to throw an exception to cause it to fail.
        Interceptor = new HttpClientInterceptorOptions().ThrowsOnMissingRegistration();
    }

    public HttpClientInterceptorOptions Interceptor { get; }

    public ITestOutputHelper? OutputHelper { get; set; }

    public void ClearOutputHelper()
        => OutputHelper = null;

    public void SetOutputHelper(ITestOutputHelper value)
        => OutputHelper = value;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(configBuilder =>
        {
            // Configure the test fixture to write the SQLite database
            // to a temporary directory, rather than in App_Data.
            var dataDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            if (!Directory.Exists(dataDirectory))
            {
                Directory.CreateDirectory(dataDirectory);
            }

            // Also override the default options for the GitHub OAuth provider
            var config = new[]
            {
                KeyValuePair.Create<string, string?>("DataDirectory", dataDirectory),
                KeyValuePair.Create<string, string?>("GitHub:ClientId", "github-id"),
                KeyValuePair.Create<string, string?>("GitHub:ClientSecret", "github-secret"),
                KeyValuePair.Create<string, string?>("GitHub:EnterpriseDomain", string.Empty)
            };

            configBuilder.AddInMemoryCollection(config);
        });

        // Route the application's logs to the xunit output
        builder.ConfigureLogging(loggingBuilder => loggingBuilder.ClearProviders().AddXUnit(this));

        // Configure the correct content root for the static content and Razor pages
        builder.UseSolutionRelativeContentRoot(Path.Combine("src", "TodoApp"), "*.slnx");

        // Configure the application so HTTP requests related to the OAuth flow
        // can be intercepted and redirected to not use the real GitHub service.
        builder.ConfigureServices(services =>
        {
            services.AddHttpClient();

            services.AddSingleton<IHttpMessageHandlerBuilderFilter, HttpRequestInterceptionFilter>(
                _ => new HttpRequestInterceptionFilter(Interceptor));

            services.AddSingleton<IPostConfigureOptions<GitHubAuthenticationOptions>, RemoteAuthorizationEventsFilter>();
            services.AddScoped<LoopbackOAuthEvents>();
        });

        // Configure a bundle of HTTP requests to intercept for the OAuth flow.
        Interceptor.RegisterBundle("oauth-http-bundle.json");
    }
}
