# dotnet-playground

[![Build](https://github.com/smaugcoath/dotnet-playground/actions/workflows/build.yml/badge.svg?branch=master)](https://github.com/smaugcoath/dotnet-playground/actions/workflows/build.yml)

A proof-of-concept exploring .NET Aspire orchestration around a Clean
Architecture minimal API, used as a sandbox for design decisions rather than a
product.

## What this demonstrates

- **Aspire orchestration with a startup dependency.** The AppHost declares
  Postgres, Redis, and the API as resources, and gates the API on the
  Postgres database and Redis being ready before it starts (`src/Pocs.Aspire.AppHost/Program.cs`).
- **Clean Architecture with dependencies pointing inward.** `Domain` carries no
  project reference to `Infrastructure` or `ApiService` and depends on no
  EF Core or ASP.NET package (`src/Pocs.Aspire.Domain/Pocs.Aspire.Domain.csproj`).
- **Expected failures as values, not exceptions.** Business services return
  `LanguageExt.Either<Failure, TResponse>`; endpoints pattern-match the result
  into the matching HTTP status (`src/Pocs.Aspire.Domain/Errors/Errors.cs`,
  `src/Pocs.Aspire.ApiService/Endpoints/UsersEndpoints.cs`).
- **Value objects with EF Core value conversions.** Identity, names, and email
  are typed value objects, mapped to plain columns via `HasConversion`
  (`src/Pocs.Aspire.Domain/Users/ValueObjects/`,
  `src/Pocs.Aspire.Infrastructure/Persistence/Configurations/UserConfiguration.cs`).
- **EF Core migrations applied at startup**, not `EnsureCreated`
  (`context.Database.Migrate()` in
  `src/Pocs.Aspire.Infrastructure/HostApplicationBuilderCollectionExtensions.cs`).
- **URL-segment API versioning**, routes live under `api/v1/users`
  (`src/Pocs.Aspire.ApiService/Endpoints/UsersEndpoints.cs`).
- **Redis output caching on a read route**, keyed with `SetVaryByRouteValue("id")`
  on the single-user GET (`src/Pocs.Aspire.ApiService/Endpoints/UsersEndpoints.cs`).
- **Architecture tests enforce the dependency direction.** ArchUnitNET rules
  fail the build if Domain or Business gain a dependency they must not have
  (`tests/Pocs.Aspire.Tests.Architecture`).
- **Three test layers, weighted toward the real thing.** Functional tests run
  the actual Aspire app via `Aspire.Hosting.Testing`; integration tests hit a
  real Postgres via Testcontainers; unit tests (NSubstitute) are reserved for
  guard clauses a running app can't reach — all asserting whole objects with
  Shouldly (`tests/`, `.claude/rules/testing.md`).

## Run it

Prerequisites:

- .NET 10 SDK
- Docker or Podman

```shell
dotnet run --project src/Pocs.Aspire.AppHost
```

The Aspire dashboard URL (with its login token) is printed to the console; the
API's Scalar reference (`/scalar`) is linked from the dashboard.

```shell
dotnet test --solution Pocs.Aspire.sln
```

Integration and functional tests need Docker running — they start real
Postgres containers.

Or open it in a dev container: `.devcontainer/` (VS Code / GitHub Codespaces)
restores tools and packages on create. `.claude/` holds the agent rules and a
session hook for Claude Code cloud sessions.

## Solution layout

- `src/Pocs.Aspire.AppHost` — Aspire orchestration entry point; declares
  Postgres, Redis, and the API resources. [README](src/Pocs.Aspire.AppHost/README.md)
- `src/Pocs.Aspire.ApiService` — Minimal API endpoints, versioning, OpenAPI,
  output caching. [README](src/Pocs.Aspire.ApiService/README.md)
- `src/Pocs.Aspire.Business` — one folder per use case (`Users/Create`,
  `Users/GetById`, `Users/Update`, `Users/Delete`, `Users/List`), each with its
  service, validator, and mapper.
- `src/Pocs.Aspire.Domain` — entities, value objects, `Failure` types,
  repository and unit-of-work abstractions.
- `src/Pocs.Aspire.Infrastructure` — EF Core `AppDbContext`, migrations,
  entity configurations, repository implementations.
  [README](src/Pocs.Aspire.Infrastructure/README.md)
- `src/Pocs.Aspire.ServiceDefaults` — shared OpenTelemetry, health check, and
  resilience wiring, referenced by every service.
- `tests/Pocs.Aspire.ApiService.Tests.Functional` — real Aspire app via
  `AspireHostFixture`, real HTTP calls. [README](tests/Pocs.Aspire.ApiService.Tests.Functional/README.md)
- `tests/Pocs.Aspire.Infrastructure.Tests.Integration` — real Postgres via
  Testcontainers. [README](tests/Pocs.Aspire.Infrastructure.Tests.Integration/README.md)
- `tests/Pocs.Aspire.Business.Tests.Unit` — NSubstitute mocks, guard clauses only.
- `tests/Pocs.Aspire.Tests.Architecture` — ArchUnitNET rules over the compiled
  assemblies: dependencies point inward, Domain and Business stay free of EF
  Core and ASP.NET Core.

## Request flow

A `POST /api/v1/users` hits `UsersEndpoints.Create`, which calls
`ICreateService.CreateAsync`. The service runs the FluentValidation validator
first; a failed validation short-circuits into a `ValidationError`. It then
checks the repository for an existing user with the same email; a match
returns `EmailAlreadyExistsError`. Otherwise it builds the `User` entity,
persists it through `IUserRepository` and `IUnitOfWork.SaveChangesAsync`
(EF Core against Postgres), and returns a `CreateResponse`. The endpoint
pattern-matches that `Either<Failure, CreateResponse>` into `201 Created`
(with a `Location` pointing at `GetById`), `400 ValidationProblem`, or
`409 Conflict`.

`DELETE /api/v1/users/{id}` and `GET /api/v1/users?page=&pageSize=` follow the
same shape: `UsersEndpoints.Delete` calls `IDeleteService.DeleteAsync`, which
validates the id, loads the user, removes it, and returns `204 NoContent`,
`400 ValidationProblem`, or `404 Not Found`.
`UsersEndpoints.List` calls `IListService.ListAsync`, which validates `page`
(1-based) and `pageSize` (1-100, default 20), then returns a `ListResponse`
of users ordered by email, or `400 ValidationProblem`.

## Decisions

- **Functional error handling over exceptions.** Expected failures
  (`ValidationError`, `EmailAlreadyExistsError`, `NotFoundError`) are modeled
  as `Either<Failure, T>` values and pattern-matched at the endpoint, so the
  failure path is visible in the method signature. Trade-off: every endpoint
  carries a `switch` over `result.Case` that has to cover every `Failure` type.
- **Value objects over primitives.** `UserId`, `FirstName`, `LastName`, and
  `Email` validate on construction instead of relying on scattered checks.
  Trade-off: each one needs an explicit EF Core `HasConversion` to and from
  its primitive column type.
- **Migrations over `EnsureCreated`.** Schema changes are versioned and
  applied with `context.Database.Migrate()` at startup. Trade-off: adding a
  column means generating and committing a migration, not just editing the
  entity.
- **Output cache keyed by route value.** Only the single-user GET is cached,
  for 5 seconds, varied by the `id` route value, so cached entries can't leak
  across users. Trade-off: a short, fixed TTL rather than active invalidation
  on write.
- **URL-segment versioning.** `api/v{version}/users` makes the active version
  explicit in the URL and in Swagger. Trade-off: bumping a version changes the
  route itself, unlike a header- or query-string-based scheme.
- **Functional-first testing.** Confidence comes mostly from the functional
  suite (real Aspire host) and the integration suite (real Postgres); unit
  tests are reserved for guard clauses the running app can't structurally
  reach. Trade-off: most of the suite needs Docker and is slower than pure
  unit tests.
- **Central package management with analyzers as errors.** `Directory.Packages.props`
  pins every NuGet version once; `Directory.Build.props` turns on
  `AnalysisMode=All` with SonarAnalyzer and `TreatWarningsAsErrors`.
  Trade-off: a new analyzer rule on an SDK bump can break the build until
  addressed.
- **Shouldly over FluentAssertions.** FluentAssertions v8+ requires a
  commercial license; Shouldly does not. Trade-off: a smaller assertion API
  and less community content to lean on.
- **Native OpenAPI + Scalar over Swashbuckle.** Swashbuckle left the
  templates in .NET 9; the built-in `Microsoft.AspNetCore.OpenApi` generator
  is maintained with the framework, and `Asp.Versioning.OpenApi` wires it to
  produce one document per API version. Trade-off: fewer customisation
  filters than Swashbuckle offered.

## How this repository is developed

This repository is developed with AI-assisted workflows. The rules the agent
follows live in `AGENTS.md` and `.claude/rules/`. Commits and pull requests
that an AI tool authored or materially contributed to carry it as co-author.
Humans decide, review, and merge.

## Roadmap

- Authentication and authorization — no identity provider or auth middleware
  is wired in yet
- A second service with inter-service messaging, to explore that side of Aspire
- Mutation testing, to check how much the current test suite actually catches

## Stack

- .NET 10 (`net10.0`)
- .NET Aspire 13.5.4 (`Aspire.AppHost.Sdk`, `Aspire.Hosting.PostgreSQL`,
  `Aspire.Hosting.Redis`, `Aspire.Npgsql.EntityFrameworkCore.PostgreSQL`,
  `Aspire.StackExchange.Redis.OutputCaching`)
- EF Core 10.0.12 (`Microsoft.EntityFrameworkCore`, `.Relational`, `.Design`),
  `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.3
- LanguageExt.Core 4.4.9
- FluentValidation 12.1.1
- Asp.Versioning.Http 10.2.3, Asp.Versioning.Mvc.ApiExplorer 10.2.1,
  Asp.Versioning.OpenApi 10.2.3
- Microsoft.AspNetCore.OpenApi 10.0.12, Scalar.AspNetCore 2.17.5
- OpenTelemetry (core/exporter 1.19.0, ASP.NET Core / HTTP / runtime
  instrumentation 1.18.0)
- xunit.v3 4.0.1, on Microsoft Testing Platform (`dotnet test` opts in via
  `global.json`)
- Microsoft.Testing.Extensions.CodeCoverage 18.11.2, ReportGenerator 5.5.11 (coverage
  summary on every CI run)
- Shouldly 4.3.0
- NSubstitute 6.2.0
- Testcontainers / Testcontainers.PostgreSql 4.15.0
- SonarAnalyzer.CSharp 10.34.0.3385

## License

MIT — see [LICENSE](LICENSE).
