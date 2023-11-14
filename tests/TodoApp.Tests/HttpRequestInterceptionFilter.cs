// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using JustEat.HttpClientInterception;
using Microsoft.Extensions.Http;

namespace TodoApp;

// See https://github.com/justeat/httpclient-interception#registering-request-interception-when-using-ihttpclientfactory

public sealed class HttpRequestInterceptionFilter(HttpClientInterceptorOptions options) : IHttpMessageHandlerBuilderFilter
{
    public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
    {
        return builder =>
        {
            next(builder);
            builder.AdditionalHandlers.Add(options.CreateHttpMessageHandler());
        };
    }
}
