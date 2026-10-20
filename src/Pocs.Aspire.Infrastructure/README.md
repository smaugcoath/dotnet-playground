# Pocs.Aspire.Infrastructure

## Overview

The Infrastructure project implements the technical concerns of the Pocs.Aspire application, including data persistence, external service integration, and cross-cutting concerns. It provides concrete implementations of the interfaces defined in the Domain layer.

## Architectural Responsibility

This project is responsible for:

- Implementing repository interfaces from the Domain layer
- Providing database access with Entity Framework Core
- Managing database migrations and schema evolution
- Configuring entity mappings for the database
- Implementing cross-cutting concerns like caching and logging

## Technical Approach

The Infrastructure layer uses:

- **Entity Framework Core**: For object-relational mapping
- **Repository Pattern**: Concrete implementations of domain repositories
- **Unit of Work Pattern**: Transaction management
- **PostgreSQL**: As the database provider
- **Dependency Injection**: For service registration

## Key Features

1. **Database Access**:
   - Entity Framework Core integration
   - PostgreSQL database support
   - Repository implementations
   - Migration support

2. **Persistence Concerns**:
   - Data mapping strategies
   - Query optimization
   - Database schema management

3. **Separation of Concerns**:
   - Clean separation between domain and database models
   - Implementation details hidden from domain

## Design Considerations

- Infrastructure concerns are isolated from domain logic
- Repository implementations follow consistent patterns
- Database-specific code is contained within this layer


## How to create and apply migrations

Open the command line tool and navigate to the repository root. The `dotnet-ef`
tool is restored from the local tool manifest, not installed globally, so run
`dotnet tool restore` once before the commands below.

### Create a new migration

```shell
dotnet tool restore
dotnet ef migrations add <NameOfYourMigration> --project ./src/Pocs.Aspire.Infrastructure --startup-project ./src/Pocs.Aspire.AppHost --output-dir Persistence/Migrations
```

### Remove migrations

Note: To remove migrations EF requires a running database to check which migrations have been applied. While working locally this can be forced with the "--force" flag.
```shell
dotnet tool restore
dotnet ef migrations remove --project ./src/Pocs.Aspire.Infrastructure --startup-project ./src/Pocs.Aspire.AppHost --force
```

### Update database

At this moment the database is automatically updated by the Aspire Host project.