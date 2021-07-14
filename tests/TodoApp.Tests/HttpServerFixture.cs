// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TodoApp;

public sealed class HttpServerFixture : TodoAppFixture, IAsyncLifetime
{
    private IHost? _host;
    private bool _disposed;

    public string ServerAddress => ClientOptions.BaseAddress.ToString();

    public override IServiceProvider Services => _host?.Services!;

    Task IAsyncLifetime.InitializeAsync()
    {
        EnsureHttpServer();
        return Task.CompletedTask;
    }

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

    private void EnsureHttpServer()
    {
        if (_host is null)
        {
            CreateHttpServer();
        }
    }

    private void CreateHttpServer()
    {
        var builder = CreateHostBuilder();

        // HACK Workaround for https://github.com/dotnet/aspnetcore/issues/33846
        if (builder is null)
        {
            builder = CreateDeferredHostBuilder();
        }

        _host = builder
            .ConfigureWebHost(ConfigureWebHost)
            .Build();

        _host.Start();

        var server = _host.Services.GetRequiredService<IServer>();
        var addresses = server.Features.Get<IServerAddressesFeature>();

        ClientOptions.BaseAddress = addresses!.Addresses
            .Select((p) => new Uri(p))
            .Last();
    }

    private static IHostBuilder CreateDeferredHostBuilder()
    {
        // From https://github.com/dotnet/aspnetcore/blob/dbf84eaa5a8f79947647ad64a785d39e7cd23afe/src/Mvc/Mvc.Testing/src/WebApplicationFactory.cs#L162-L174
        var instanceMethod = BindingFlags.Instance | BindingFlags.Public;
        var staticMethod = BindingFlags.Public | BindingFlags.Static;

        var deferredHostBuilderType = Type.GetType("Microsoft.AspNetCore.Mvc.Testing.DeferredHostBuilder, Microsoft.AspNetCore.Mvc.Testing");
        var constructor = deferredHostBuilderType!.GetConstructor(instanceMethod, Type.EmptyTypes);
        var setHostFactory = deferredHostBuilderType.GetMethod("SetHostFactory", instanceMethod);
        var configureHostBuilderMethod = deferredHostBuilderType.GetMethod("ConfigureHostBuilder", instanceMethod);
        var entryPointCompletedMethod = deferredHostBuilderType.GetMethod("EntryPointCompleted", instanceMethod);

        var hostFactoryResolverType = Type.GetType("Microsoft.Extensions.Hosting.HostFactoryResolver, Microsoft.AspNetCore.Mvc.Testing");
        var resolveHostFactory = hostFactoryResolverType!.GetMethod("ResolveHostFactory", staticMethod);

        var deferredHostBuilder = (IHostBuilder)constructor!.Invoke(Array.Empty<object>());

        Action<object>? configureHostBuilder = (Action<object>?)Delegate.CreateDelegate(typeof(Action<object>), deferredHostBuilder, configureHostBuilderMethod!);
        Action<Exception?>? entrypointCompleted = (Action<Exception?>?)Delegate.CreateDelegate(typeof(Action<Exception?>), deferredHostBuilder, entryPointCompletedMethod!);

        var factory = (Func<string[], object>)resolveHostFactory!.Invoke(null, new object?[]
        {
            typeof(Services.ITodoService).Assembly,
            null,
            false,
            configureHostBuilder,
            entrypointCompleted,
        })!;

        if (factory is null)
        {
            throw new NotSupportedException("Failed to resolve entry point for web application.");
        }

        setHostFactory!.Invoke(deferredHostBuilder, new object[] { factory });

        return deferredHostBuilder.UseEnvironment(Environments.Development);
    }
}
