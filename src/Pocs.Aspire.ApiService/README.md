# Pocs.Aspire.ApiService

## Overview

The ApiService project is the ASP.NET Core Minimal API that exposes the
`Users` use cases to HTTP clients. It hosts the endpoint definitions, wires up
API versioning, Swagger, Redis output caching, and Problem Details, and
delegates all business logic to `Pocs.Aspire.Business`.

## Architectural Responsibility

This project is responsible for:

- Exposing REST endpoints for client applications (`Endpoints/UsersEndpoints.cs`)
- Pattern-matching each use case's `Either<Failure, TResponse>` result into the
  matching HTTP response
- Declaring API versioning
- Providing OpenAPI documentation
- Applying output caching to cacheable routes

## Technical Approach

The ApiService uses:

- **Minimal APIs**: endpoint groups defined as static methods, no controllers
- **Asp.Versioning.Http**: URL-segment versioning (`api/v{version}/...`)
- **Output Caching**: Redis-backed response caching on the read endpoint,
  varied by route value
- **Problem Details**: [RFC 7807](https://datatracker.ietf.org/doc/html/rfc7807) compliant error responses, with request and trace IDs attached
- **Swagger/OpenAPI**: via Swashbuckle, served in the Development environment

## Third-Party Packages

- [**Asp.Versioning.Http**](https://github.com/dotnet/aspnet-api-versioning/wiki): API versioning support
- [**Aspire.StackExchange.Redis.OutputCaching**](https://www.nuget.org/packages/Aspire.StackExchange.Redis.OutputCaching): Redis-backed output caching, wired by Aspire
- [**Swashbuckle.AspNetCore**](https://github.com/domaindrivendev/Swashbuckle.AspNetCore): Swagger/OpenAPI documentation

## Design Considerations

- No domain or DTO mapping happens here — that lives in `Pocs.Aspire.Business`
- Consistent request/response patterns across all endpoints
- Caching is opt-in per route, not applied blanket-wide
