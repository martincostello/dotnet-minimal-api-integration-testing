To contribute changes (source code, scripts, configuration) to this repository please follow the steps below.
These steps are a guideline for contributing and do not necessarily need to be followed for all changes.

 1. If you intend to fix a bug please create an issue before forking the repository.
 1. Fork the `main` branch of this repository from the latest commit.
 1. Create a branch from your fork's `main` branch to help isolate your changes from any further work on `main`. If fixing an issue try to reference its name in your branch name (e.g. `issue-123`) to make changes easier to track the changes.
 1. Work on your proposed changes on your fork. If you are fixing an issue include at least one unit test that reproduces it if the code changes to fix it have not been applied; if you are adding new functionality please include unit tests appropriate to the changes you are making.
 1. When you think your changes are complete, test that the code builds cleanly using `build.ps1`/`build.sh`. There should be no compiler warnings and all tests should pass.
 1. Once your changes build cleanly locally submit a Pull Request back to the `main` branch from your fork's branch. Ideally commits to your branch should be squashed before creating the Pull Request. If the Pull Request fixes an issue please reference it in the title and/or description. Please keep changes focused around a specific topic rather than include multiple types of changes in a single Pull Request.
 1. After your Pull Request is created it will build against the repository's continuous integrations.
 1. Once the Pull Request has been reviewed by the project's [contributors](https://github.com/martincostello/dotnet-minimal-api-integration-testing/graphs/contributors) and the status checks pass your Pull Request will be merged back to the `main` branch, assuming that the changes are deemed appropriate.
