// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TodoApp
{
    public sealed class HttpServerFixture : TodoAppFixture, IAsyncLifetime
    {
        private IHost? _host;
        private bool _disposed;

        public string ServerAddress => ClientOptions.BaseAddress.ToString();

        public override IServiceProvider Services => _host?.Services!;

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

            builder.ConfigureKestrel(
                serverOptions => serverOptions.ConfigureHttpsDefaults(
                    httpsOptions => httpsOptions.ServerCertificate = new X509Certificate2("localhost-dev.pfx", "Pa55w0rd!")));

            // Configure the server address for the server to
            // listen on for HTTPS requests on a dynamic port.
            builder.UseUrls("https://127.0.0.1:0");
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
            if (_host is null)
            {
                await CreateHttpServerAsync();
            }
        }

        private async Task CreateHttpServerAsync()
        {
            _host = CreateHostBuilder()!
                .ConfigureWebHost(ConfigureWebHost)
                .Build();

            // Force creation of the Kestrel server and start it
            var hostedService = _host.Services.GetService<IHostedService>();
            await hostedService!.StartAsync(default);

            var server = _host.Services.GetRequiredService<IServer>();
            var addresses = server.Features.Get<IServerAddressesFeature>();

            ClientOptions.BaseAddress = addresses!.Addresses
                .Select((p) => new Uri(p))
                .Last();
        }
    }
}
