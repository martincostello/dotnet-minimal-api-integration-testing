// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNetCore.Http
{
    // Will be redundant when changes are available in a later preview.
    // See https://github.com/dotnet/aspnetcore/pull/33843.

    internal sealed class Results
    {
        public static IResult BadRequest()
            => new BadRequestResult();

        public static IResult Challenge(AuthenticationProperties properties, params string[] authenticationSchemes)
            => new ChallengeResult(authenticationSchemes, properties);

        public static IResult Created(string url, object? value)
            => new CreatedAtResult(url, value);

        public static IResult Json(object? data)
            => new JsonResult(data);

        public static IResult NoContent()
            => new NoContentResult();

        public static IResult NotFound()
            => new NotFoundResult();

        public static IResult Redirect(string url)
            => new RedirectResult(url);

        public static IResult SignOut(AuthenticationProperties properties, params string[] authenticationSchemes)
            => new SignOutResult(authenticationSchemes, properties);

        public static IResult StatusCode(int statusCode)
            => new StatusCodeResult(statusCode);

        // HACK Custom result types until CreatedAtResult/ObjectResult implements IResult.
        // See https://github.com/dotnet/aspnetcore/issues/32565
        // and https://github.com/dotnet/aspnetcore/pull/33843.

        private sealed class CreatedAtResult : IResult
        {
            private readonly string _location;
            private readonly object? _value;

            internal CreatedAtResult(string location, object? value)
            {
                _location = location;
                _value = value;
            }

            public async Task ExecuteAsync(HttpContext httpContext)
            {
                var response = httpContext.Response;

                response.Headers.Location = _location;
                response.StatusCode = StatusCodes.Status201Created;

                await response.WriteAsJsonAsync(_value, httpContext.RequestAborted);
            }
        }
    }
}
