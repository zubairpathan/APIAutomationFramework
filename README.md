# AutomationFramework

This repository contains a C# REST automation framework with:

- `ApiClient` project: a thin RestSharp-based HTTP client.
- `ApiTests` project: xUnit tests that exercise the REST API and includes test fixtures under `testdata/objects`.
- GitHub Actions workflow in `.github/workflows/ci.yml` that runs the tests on push/PR.

## Prerequisites

- .NET 9 SDK (install from https://dot.net)
- Git (to clone the repo)
- Optional: Visual Studio 2022/2023 or VS Code

All other dependencies (NuGet packages) are restored automatically via `dotnet restore`.

## How to run locally (Windows PowerShell)

Open PowerShell in the repo root and run:

```powershell
# Restore dependencies
dotnet restore

# Build the solution
dotnet build --configuration Release

# Run tests (runs ApiTests)
dotnet test --verbosity normal
```

Notes:
- Tests call a live API endpoint configured via `appsettings.json` in the `ApiTests` project. Update that file or the environment variables to point to your target environment.
- Test artifacts (TRX) are created under `ApiTests/TestResults` and are also uploaded by the CI pipeline.

## Continuous Integration

A GitHub Actions workflow `.github/workflows/ci.yml` runs on push and pull_request (`master`) and executes:

- dotnet restore
- dotnet build
- dotnet test (produces TRX)
- uploads TRX artifacts
- publishes TRX results into GitHub Checks for annotations using `dorny/test-reporter`

## Test data

Fixtures are stored in `ApiTests/testdata/objects`. Tests read these files via `ApiTests.Utils.TestData`.

## Extending

- Add more tests to `ApiTests/Tests`.
- Add caching to CI workflow for faster runs.

## Troubleshooting

- If tests fail due to 405 Method Not Allowed on reserved ids (e.g., id=7) the tests intentionally exercise both happy and negative paths; see `ObjectsTests` comments.

---