# Pocs.Aspire.AppHost

## Overview

The AppHost project is the .NET Aspire orchestration entry point. `Program.cs`
declares the resources the distributed application needs — a Postgres
container, a Redis container, and the ApiService project — and how they are
wired together.

## Architectural Responsibility

This project is responsible for:

- Declaring the Postgres, Redis, and ApiService resources
- Passing connection references (`postgresdb`, `cache`) into ApiService
- Gating ApiService startup on the Postgres database and Redis being ready
  (`WaitFor(postgresDb)`, `WaitFor(cache)`)
- Launching the Aspire dashboard for local development

## Technical Approach

`Program.cs` builds the application graph with `DistributedApplication.CreateBuilder`:

- `AddRedis("cache")` with RedisInsight enabled
- `AddPostgres("postgres")` (image `postgres:15`) with pgAdmin enabled, and an
  `AddDatabase("postgresdb")` database on top of it
- `AddProject<Projects.Pocs_Aspire_ApiService>("apiservice")`, referencing both
  resources and waiting for both to be healthy before starting

## Usage

To start the entire application, run:

```shell
dotnet run --project src/Pocs.Aspire.AppHost
```

The Aspire dashboard URL (with its login token) is printed to the console. The
dashboard shows every resource's status and links to the ApiService's Scalar
API reference.
