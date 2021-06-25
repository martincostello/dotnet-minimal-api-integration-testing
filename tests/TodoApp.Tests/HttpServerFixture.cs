// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using AspNet.Security.OAuth.GitHub;
using JustEat.HttpClientInterception;
using MartinCostello.Logging.XUnit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TodoApp
{
    public sealed class HttpServerFixture : WebApplicationFactory<Services.ITodoService>, IAsyncLifetime, ITestOutputHelperAccessor
    {
        private IHost? _host;
        private bool _disposed;

        public HttpServerFixture()
            : base()
        {
            ClientOptions.AllowAutoRedirect = false;
            ClientOptions.BaseAddress = new Uri("https://localhost");
            Interceptor = new HttpClientInterceptorOptions().ThrowsOnMissingRegistration();
        }

        public HttpClientInterceptorOptions Interceptor { get; }

        public ITestOutputHelper? OutputHelper { get; set; }

        public string ServerAddress => ClientOptions.BaseAddress.ToString();

        public override IServiceProvider Services => _host?.Services!;

        public void ClearOutputHelper()
            => OutputHelper = null;

        public void SetOutputHelper(ITestOutputHelper value)
            => OutputHelper = value;

        async Task IAsyncLifetime.InitializeAsync()
            => await EnsureHttpServerAsync();

        async Task IAsyncLifetime.DisposeAsync()
        {
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
                _host = null;
            }
        }

        public HttpClient CreateHttpClient()
        {
            var handler = new HttpClientHandler()
            {
                AllowAutoRedirect = ClientOptions.AllowAutoRedirect,
                CheckCertificateRevocationList = true,
                MaxAutomaticRedirections = ClientOptions.MaxAutomaticRedirections,
                UseCookies = ClientOptions.HandleCookies,
            };

            try
            {
                var client = new HttpClient(handler, disposeHandler: true);

                ConfigureClient(client);

                client.BaseAddress = ClientOptions.BaseAddress;

                return client;
            }
            catch (Exception)
            {
                handler.Dispose();
                throw;
            }
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);

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

                builder.Sources.Clear();

                builder.AddJsonFile(fullPath)
                       .AddInMemoryCollection(config);
            });

            builder.ConfigureKestrel(
                (serverOptions) => serverOptions.ConfigureHttpsDefaults(
                    (httpsOptions) => httpsOptions.ServerCertificate = new X509Certificate2("localhost-dev.pfx", "Pa55w0rd!")));

            builder.ConfigureLogging((loggingBuilder) => loggingBuilder.ClearProviders().AddXUnit(this))
                   .UseContentRoot(GetApplicationContentRootPath());

            builder.ConfigureServices((services) =>
            {
                services.AddSingleton<IHttpMessageHandlerBuilderFilter, HttpRequestInterceptionFilter>(
                    (_) => new HttpRequestInterceptionFilter(Interceptor));

                services.AddSingleton<IPostConfigureOptions<GitHubAuthenticationOptions>, RemoteAuthorizationEventsFilter>();
            });

            // Configure the server address for the server to
            // listen on for HTTPS requests on a dynamic port.
            builder.UseUrls("https://127.0.0.1:0");

            Interceptor.RegisterBundle("oauth-http-bundle.json");
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!_disposed)
            {
                if (disposing)
                {
                    _host?.Dispose();
                }

                _disposed = true;
            }
        }

        private async Task EnsureHttpServerAsync()
        {
            if (_host == null)
            {
                await CreateHttpServer();
            }
        }

        private async Task CreateHttpServer()
        {
            var builder = CreateHostBuilder()!.ConfigureWebHost(ConfigureWebHost);

            _host = builder.Build();

            // Force creation of the Kestrel server and start it
            var hostedService = _host.Services.GetService<IHostedService>();
            await hostedService!.StartAsync(default);

            var server = _host.Services.GetRequiredService<IServer>();

            ClientOptions.BaseAddress = server.Features.Get<IServerAddressesFeature>()!.Addresses
                .Select((p) => new Uri(p))
                .First();
        }

        private string GetApplicationContentRootPath()
        {
            var attribute = GetTestAssemblies()
                .SelectMany((p) => p.GetCustomAttributes<WebApplicationFactoryContentRootAttribute>())
                .Where((p) => string.Equals(p.Key, "TodoApp", StringComparison.OrdinalIgnoreCase))
                .OrderBy((p) => p.Priority)
                .First();

            return attribute.ContentRootPath;
        }
    }
}
