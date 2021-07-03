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
        private static readonly ApiExplorerSettingsAttribute _ignoreApiMetadata = new ApiExplorerSettingsAttribute
        {
            IgnoreApi = true
        };

        /// <summary>
        /// Ignores the endpoint(s) from the API Explorer.
        /// </summary>
        /// <param name="builder">The endpoint convention builder.</param>
        /// <returns>The original convention builder parameter.</returns>
        public static TBuilder IgnoreApi<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder
        {
            builder.Add(endpointBuilder =>
            {
                endpointBuilder.Metadata.Add(_ignoreApiMetadata);
            });
            return builder;
        }
    }
}
