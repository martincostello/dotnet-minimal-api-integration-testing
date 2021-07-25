// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.IO;
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

namespace TodoApp
{
    public class TodoAppFixture : WebApplicationFactory<Services.ITodoService>, ITestOutputHelperAccessor
    {
        public TodoAppFixture()
            : base()
        {
            ClientOptions.AllowAutoRedirect = false;
            ClientOptions.BaseAddress = new Uri("https://localhost");
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
            // HACK Workaround for https://github.com/dotnet/aspnetcore/issues/33889
            builder.UseEnvironment(Environments.Development);

            builder.ConfigureAppConfiguration((builder) =>
            {
                string dataDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

                if (!Directory.Exists(dataDirectory))
                {
                    Directory.CreateDirectory(dataDirectory);
                }

                var config = new[]
                {
                    KeyValuePair.Create("DataDirectory", dataDirectory),
                };

                string? directory = Path.GetDirectoryName(typeof(HttpServerFixture).Assembly.Location);
                string fullPath = Path.Combine(directory ?? ".", "testsettings.json");

                builder.AddJsonFile(fullPath)
                       .AddInMemoryCollection(config);
            });

            builder.ConfigureLogging((loggingBuilder) => loggingBuilder.ClearProviders().AddXUnit(this))
                   .UseSolutionRelativeContentRoot(Path.Combine("src", "TodoApp"));

            builder.ConfigureServices((services) =>
            {
                services.AddSingleton<IHttpMessageHandlerBuilderFilter, HttpRequestInterceptionFilter>(
                    (_) => new HttpRequestInterceptionFilter(Interceptor));

                services.AddSingleton<IPostConfigureOptions<GitHubAuthenticationOptions>, RemoteAuthorizationEventsFilter>();
            });

            Interceptor.RegisterBundle("oauth-http-bundle.json");
        }
    }
}
