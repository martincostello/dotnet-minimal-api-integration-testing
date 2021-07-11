// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// API Explorer extension methods for <see cref="IEndpointConventionBuilder"/>.
    /// </summary>
    public static class ApiExplorerEndpointConventionBuilderExtensions
    {
        private static readonly ApiExplorerSettingsAttribute _ignoreApiMetadata = new()
        {
            IgnoreApi = true
        };

        /// <summary>
        /// Ignores the endpoint(s) from the API Explorer.
        /// </summary>
        /// <typeparam name="TBuilder">The type of the endpoint builder.</typeparam>
        /// <param name="builder">The endpoint convention builder.</param>
        /// <returns>The original convention builder parameter.</returns>
        public static TBuilder IgnoreApi<TBuilder>(this TBuilder builder)
            where TBuilder : IEndpointConventionBuilder
        {
            builder.Add(endpointBuilder =>
            {
                endpointBuilder.Metadata.Add(_ignoreApiMetadata);
            });
            return builder;
        }

        /// <summary>
        /// Specifies status code returned by the endpoint.
        /// </summary>
        /// <typeparam name="TBuilder">The type of the endpoint builder.</typeparam>
        /// <param name="builder">The endpoint convention builder.</param>
        /// <param name="statusCode">The HTTP response status code.</param>
        /// <returns>The original convention builder parameter.</returns>
        public static TBuilder ProducesStatusCode<TBuilder>(this TBuilder builder, int statusCode)
            where TBuilder : IEndpointConventionBuilder
        {
            return builder.ProducesStatusCode(statusCode, typeof(void));
        }

        /// <summary>
        /// Specifies status code and the type of the value returned by the endpoint.
        /// </summary>
        /// <typeparam name="TBuilder">The type of the endpoint builder.</typeparam>
        /// <param name="builder">The endpoint convention builder.</param>
        /// <param name="statusCode">The HTTP response status code.</param>
        /// <param name="type">The type of object that is going to be written in the response.</param>
        /// <returns>The original convention builder parameter.</returns>
        public static TBuilder ProducesStatusCode<TBuilder>(this TBuilder builder, int statusCode, Type type)
            where TBuilder : IEndpointConventionBuilder
        {
            builder.Add(endpointBuilder =>
            {
                endpointBuilder.Metadata.Add(new ProducesResponseTypeAttribute(type, statusCode));
            });
            return builder;
        }

        /// <summary>
        /// Specifies the error status code returned by the endpoint.
        /// </summary>
        /// <typeparam name="TBuilder">The type of the endpoint builder.</typeparam>
        /// <param name="builder">The endpoint convention builder.</param>
        /// <param name="statusCode">The HTTP response status code.</param>
        /// <returns>The original convention builder parameter.</returns>
        public static TBuilder ProducesErrorStatusCode<TBuilder>(this TBuilder builder, int statusCode)
            where TBuilder : IEndpointConventionBuilder
        {
            builder.Add(endpointBuilder =>
            {
                endpointBuilder.Metadata.Add(new ProducesResponseTypeAttribute(typeof(ProblemDetails), statusCode));
            });
            return builder;
        }
    }
}
