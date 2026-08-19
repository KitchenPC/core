# Contributing to KitchenPC Core

Thank you for contributing to KitchenPC. Please open an issue before starting a substantial change so the design and scope can be discussed.

## Development workflow

1. Fork the repository and create a focused branch from `main`.
2. Make the smallest change that solves the issue.
3. Add or update tests when behavior changes.
4. Restore, build, and test before opening a pull request:

   ```bash
   dotnet restore src/core.slnx
   dotnet build src/core.slnx --configuration Release --no-restore
   dotnet test src/UnitTests/UnitTests.csproj --configuration Release --no-build --no-restore
   ```

5. Open a pull request against `main` and describe both the change and how it was tested.

Please do not include unrelated formatting or cleanup in a functional change. By contributing, you agree that your contribution will be licensed under the repository's MIT license.
