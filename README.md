# Integration Testing ASP.NET Core Minimal APIs

[![Build status](https://github.com/martincostello/dotnet-minimal-api-integration-testing/actions/workflows/build.yml/badge.svg?branch=main&event=push)](https://github.com/martincostello/dotnet-minimal-api-integration-testing/actions/workflows/build.yml?query=branch%3Amain+event%3Apush)
[![codecov](https://codecov.io/gh/martincostello/dotnet-minimal-api-integration-testing/branch/main/graph/badge.svg)](https://codecov.io/gh/martincostello/dotnet-minimal-api-integration-testing)

## Introduction

This sample project demonstrates techniques you can use for integration testing
an ASP.NET Core web application that uses the [minimal APIs] feature.

[minimal APIs]: https://devblogs.microsoft.com/aspnet/asp-net-core-updates-in-net-6-preview-4/#introducing-minimal-apis

The system-under-test used by the sample implements a simple Todo list
application with ASP.NET Core using the following technologies:

* [Minimal APIs]
* [EFCore] with [SQLite]
* [GitHub OAuth] authentication
* [Razor Pages]
* [TypeScript]

[EFCore]: https://docs.microsoft.com/en-us/ef/core/
[GitHub OAuth]: https://github.com/aspnet-contrib/AspNet.Security.OAuth.Providers/blob/dev/docs/github.md
[Razor Pages]: https://docs.microsoft.com/en-us/aspnet/core/razor-pages/
[SQLite]: https://docs.microsoft.com/en-us/ef/core/providers/sqlite/
[TypeScript]: https://www.typescriptlang.org/docs/handbook/typescript-in-5-minutes-oop.html

The tests show how you can write integration tests for the [API] and [User
Interface] layers of an application that can help you get good coverage of the
system-under-test, as well as help give you confidence that the changes you make
to an application are ready to ship to a production system.

The tests include demonstrations of the use of the following open source
libraries and technologies:

* [coverlet]
* [HttpClientFactory]
* [HttpClient Interception]
* [Playwright]
* [ReportGenerator]
* [Shouldly]
* [WebApplicationFactory&lt;T&gt;]
* [xunit]
* [xunit Logging]

[API]: https://github.com/martincostello/dotnet-minimal-api-integration-testing/blob/main/tests/TodoApp.Tests/ApiTests.cs
[coverlet]: https://github.com/coverlet-coverage/coverlet
[HttpClientFactory]: https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
[HttpClient Interception]: https://github.com/justeat/httpclient-interception
[Playwright]: https://playwright.dev/dotnet/
[ReportGenerator]: https://github.com/danielpalme/ReportGenerator
[Shouldly]: https://shouldly.io/
[User Interface]: https://github.com/martincostello/dotnet-minimal-api-integration-testing/blob/main/tests/TodoApp.Tests/UITests.cs
[WebApplicationFactory&lt;T&gt;]: https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests
[xunit]: https://xunit.net/
[xunit Logging]: https://github.com/martincostello/xunit-logging

## Debugging

To debug the application locally outside of the integration tests, you will need
to [create a GitHub OAuth app] to obtain secrets for the `GitHub:ClientId` and
`GitHub:ClientSecret` [options] so that the [OAuth user authentication] works and
you can log into the Todo App UI.

> 💡 When creating the GitHub OAuth app, use `https://localhost:50001/sign-in-github`
as the _Authorization callback URL_.
>
> ⚠️ Do not commit GitHub OAuth secrets to source control. Configure them
with [User Secrets] instead.

[create a GitHub OAuth app]: https://docs.github.com/en/developers/apps/building-oauth-apps/creating-an-oauth-app
[OAuth user authentication]: https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/?view=aspnetcore-5.0&tabs=visual-studio
[options]: https://github.com/martincostello/dotnet-minimal-api-integration-testing/blob/1cd99029a9e3af57ab2fe1335b43e298efb65c09/src/TodoApp/appsettings.json#L10-L11
[User Secrets]: https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets

## Building and Testing

Compiling the application yourself requires Git and the
[.NET SDK](https://www.microsoft.com/net/download/core "Download the .NET SDK")
to be installed (version `9.0.100` or later).

To build and test the application locally from a terminal/command-line, run the
following set of commands:

```powershell
git clone https://github.com/martincostello/dotnet-minimal-api-integration-testing.git
cd dotnet-minimal-api-integration-testing
./build.ps1
```

## Feedback

Any feedback or issues can be added to the issues for this project in
[GitHub](https://github.com/martincostello/dotnet-minimal-api-integration-testing/issues "Issues for this project on GitHub.com").

## Acknowledgements

Thanks to David Fowler ([@davidfowl](https://github.com/davidfowl)) from the
ASP.NET Core team for helping out with resolving issues with Minimal Actions
found from testing this sample with the ASP.NET Core 6 pre-releases!

## Repository

The repository is hosted in
[GitHub](https://github.com/martincostello/dotnet-minimal-api-integration-testing "This project on GitHub.com"):
<https://github.com/martincostello/dotnet-minimal-api-integration-testing.git>

## License

This project is licensed under the
[Apache 2.0](http://www.apache.org/licenses/LICENSE-2.0.txt "The Apache 2.0 license")
license.
