// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;

namespace TodoApp;

/// <summary>
/// A test server fixture that hosts the application on an HTTPS port so
/// that the application can be accessed through a browser for UI tests.
/// </summary>
public sealed class HttpServerFixture : TodoAppFixture
{
    public HttpServerFixture()
    {
        // Configure the address for the server to listen on for HTTPS
        // requests on a dynamic port. with a self-signed TLS certificate.
        UseKestrel(
            (server) => server.Listen(
                IPAddress.Loopback, 0, (listener) => listener.UseHttps(
                    (https) => https.ServerCertificate = X509CertificateLoader.LoadPkcs12FromFile("localhost-dev.pfx", "Pa55w0rd!"))));
    }

    public string ServerAddress
    {
        get
        {
            StartServer();
            return ClientOptions.BaseAddress.ToString();
        }
    }
}
