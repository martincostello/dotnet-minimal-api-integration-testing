# Integration Testing ASP.NET Core 6 Minimal APIs

[![Build status](https://github.com/martincostello/dotnet-minimal-api-integration-testing/workflows/build/badge.svg?branch=main&event=push)](https://github.com/martincostello/dotnet-minimal-api-integration-testing/actions?query=workflow%3Abuild+branch%3Amain+event%3Apush)

[![Open in Visual Studio Code](https://open.vscode.dev/badges/open-in-vscode.svg)](https://open.vscode.dev/martincostello/dotnet-minimal-api-integration-testing)

## Introduction

> 🛈 This project currently depends on features in daily builds of the .NET
`6.0.100-rc.1` SDK and requires preview builds of Visual Studio 2022 for IDE
support.
>
> To work with this sample directly from source with Visual Studio Code, run
`build.ps1` and then `startvscode.cmd` or `startvscode.sh`.

This sample project demonstrates techniques you can use for integration testing
an ASP.NET Core 6 web application that uses the new [minimal APIs] feature.

[minimal APIs]: https://devblogs.microsoft.com/aspnet/asp-net-core-updates-in-net-6-preview-4/#introducing-minimal-apis

The system-under-test used by the sample implements a simple Todo list
application with ASP.NET Core 6 using the following technologies:

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
Interface] layers of the application that can help you get good coverage of the
system-under-test, as well as confidence in changes you make to an application
are ready to ship to a production system.

The tests include use of the following open source technologies:

* [HttpClient Interception]
* [Playwright]
* [Shouldly]
* [WebApplicationFactory&lt;T&gt;]
* [xunit]
* [xunit Logging]

[API]: https://github.com/martincostello/dotnet-minimal-api-integration-testing/blob/main/tests/TodoApp.Tests/ApiTests.cs
[HttpClient Interception]: https://github.com/justeat/httpclient-interception
[Playwright]: https://playwright.dev/dotnet/
[Shouldly]: https://shouldly.io/
[User Interface]: https://github.com/martincostello/dotnet-minimal-api-integration-testing/blob/main/tests/TodoApp.Tests/UITests.cs
[WebApplicationFactory&lt;T&gt;]: https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests
[xunit]: https://xunit.net/
[xunit Logging]: https://github.com/martincostello/xunit-logging

## Building and Testing

Compiling the application yourself requires Git and the
[.NET SDK](https://www.microsoft.com/net/download/core "Download the .NET SDK")
to be installed (version `6.0.100` or later).

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

## Repository

The repository is hosted in
[GitHub](https://github.com/martincostello/dotnet-minimal-api-integration-testing "This project on GitHub.com"):
https://github.com/martincostello/dotnet-minimal-api-integration-testing.git

## License

This project is licensed under the
[Apache 2.0](http://www.apache.org/licenses/LICENSE-2.0.txt "The Apache 2.0 license")
license.

## Acknowledgements

Thanks to David Fowler ([@davidfowl](https://github.com/davidfowl)) from the
ASP.NET Core team for helping out with resolving issues with Minimal Actions
found from testing this sample with the ASP.NET Core 6 pre-releases!
