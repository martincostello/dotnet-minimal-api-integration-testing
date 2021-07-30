// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// API Explorer extension methods for <see cref="IEndpointConventionBuilder"/>.
/// </summary>
public static class ApiExplorerEndpointConventionBuilderExtensions
{
    //// TODO This can be removed once https://github.com/dotnet/aspnetcore/issues/34068
    //// is resolved. Also see https://github.com/dotnet/aspnetcore/pull/34860.

    private static readonly ApiExplorerSettingsAttribute _ignoreApiMetadata = new()
    {
        IgnoreApi = true
    };

    /// <summary>
    /// Ignores the endpoint(s) from the API Explorer.
    /// </summary>
    /// <param name="builder">The endpoint convention builder.</param>
    /// <returns>The original convention builder parameter.</returns>
    public static IEndpointConventionBuilder ExcludeFromApiExplorer(this IEndpointConventionBuilder builder)
    {
        builder.Add(endpointBuilder =>
        {
            endpointBuilder.Metadata.Add(_ignoreApiMetadata);
        });
        return builder;
    }

    //// TODO Methods below can be removed once https://github.com/dotnet/aspnetcore/issues/34542
    //// is resolved. Also see https://github.com/dotnet/aspnetcore/pull/34860.

    /// <summary>
    /// Adds metadata indicating the type of response an endpoint produces.
    /// </summary>
    /// <param name="builder">The endpoint convention builder.</param>
    /// <param name="statusCode">>The response status code.</param>
    /// <returns>The original convention builder parameter.</returns>
    public static IEndpointConventionBuilder Produces<TResponse>(this IEndpointConventionBuilder builder, int statusCode = StatusCodes.Status200OK)
    {
        return builder.Produces(statusCode, typeof(TResponse));
    }

    /// <summary>
    /// Adds metadata indicating the type of response an endpoint produces.
    /// </summary>
    /// <param name="builder">The endpoint convention builder.</param>
    /// <param name="statusCode">The response status code.</param>
    /// <param name="responseType">The type of the response.</param>
    /// <returns>The original convention builder parameter.</returns>
    public static IEndpointConventionBuilder Produces(this IEndpointConventionBuilder builder, int statusCode = StatusCodes.Status200OK, Type? responseType = null)
    {
        builder.Add(endpointBuilder =>
        {
            endpointBuilder.Metadata.Add(new ProducesResponseTypeAttribute(responseType ?? typeof(void), statusCode));
        });
        return builder;
    }

    /// <summary>
    /// Adds metadata indicating that the endpoint produces a Problem Details response.
    /// </summary>
    /// <param name="builder">The endpoint convention builder.</param>
    /// <param name="statusCode">The HTTP response status code.</param>
    /// <returns>The original convention builder parameter.</returns>
    public static IEndpointConventionBuilder ProducesProblem(this IEndpointConventionBuilder builder, int statusCode = StatusCodes.Status500InternalServerError)
    {
        builder.Add(endpointBuilder =>
        {
            endpointBuilder.Metadata.Add(new ProducesResponseTypeAttribute(typeof(ProblemDetails), statusCode));
        });
        return builder;
    }
}
