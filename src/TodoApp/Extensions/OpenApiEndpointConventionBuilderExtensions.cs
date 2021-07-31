// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNetCore.Http;

/// <summary>
/// Extension methods for adding response type metadata to endpoints.
/// </summary>
public static class OpenApiEndpointConventionBuilderExtensions
{
    //// TODO This can be removed once https://github.com/dotnet/aspnetcore/issues/34068
    //// is resolved. Also see https://github.com/dotnet/aspnetcore/pull/34860.

    private static readonly ApiExplorerSettingsAttribute _ignoreApiMetadata = new()
    {
        IgnoreApi = true
    };

    /// <summary>
    /// Adds metadata to support suppressing OpenAPI documentation from
    /// being generated for this endpoint.
    /// </summary>
    /// <param name="builder">The <see cref="IEndpointConventionBuilder"/>.</param>
    /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
    public static IEndpointConventionBuilder SuppressApi(this IEndpointConventionBuilder builder)
    {
        return builder.WithMetadata(_ignoreApiMetadata);
    }

    //// TODO Methods below can be removed once https://github.com/dotnet/aspnetcore/issues/34542
    //// is resolved. Also see https://github.com/dotnet/aspnetcore/pull/34860.

    /// <summary>
    /// Adds metadata indicating the type of response an endpoint produces.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="builder">The <see cref="IEndpointConventionBuilder"/>.</param>
    /// <param name="statusCode">The response status code. Defatuls to StatusCodes.Status200OK.</param>
    /// <param name="contentType">The response content type. Defaults to "application/json"</param>
    /// <param name="additionalContentTypes">Additional response content types the endpoint produces for the supplied status code.</param>
    /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
    public static IEndpointConventionBuilder Produces<TResponse>(
        this IEndpointConventionBuilder builder,
        int statusCode = StatusCodes.Status200OK)
    {
        return Produces(builder, statusCode, typeof(TResponse));
    }

    /// <summary>
    /// Adds metadata indicating the type of response an endpoint produces.
    /// </summary>
    /// <param name="builder">The <see cref="IEndpointConventionBuilder"/>.</param>
    /// <param name="statusCode">The response status code. Defaults to StatusCodes.Status200OK.</param>
    /// <param name="responseType">The type of the response. Defaults to null.</param>
    /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
    public static IEndpointConventionBuilder Produces(
        this IEndpointConventionBuilder builder,
        int statusCode = StatusCodes.Status200OK,
        Type? responseType = null)
    {
        return builder.WithMetadata(new ProducesResponseTypeAttribute(responseType ?? typeof(void), statusCode));
    }

    /// <summary>
    /// Adds metadata indicating that the endpoint produces a Problem Details response.
    /// </summary>
    /// <param name="builder">The <see cref="IEndpointConventionBuilder"/>.</param>
    /// <param name="statusCode">The response status code. Defatuls to StatusCodes.Status500InternalServerError.</param>
    /// <returns>A <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
    public static IEndpointConventionBuilder ProducesProblem(
        this IEndpointConventionBuilder builder,
        int statusCode = StatusCodes.Status500InternalServerError)
    {
        return Produces<ProblemDetails>(builder, statusCode);
    }
}
