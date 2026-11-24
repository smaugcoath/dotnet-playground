---
paths:
  - "src/**/*.cs"
---

# Implementation guidelines (production code)

**Self-explanatory code over comments.** Code must read clearly without narration.

- Do not add comments that describe *what* a line does or *how* it works. If the
  code needs that, extract a well-named private method instead (SOLID: single
  responsibility, self-documenting names).
- A rare inline `//` is acceptable only for something genuinely non-obvious (a
  magic number's origin, a framework quirk) that naming cannot express.
- `///` XML doc comments describe WHAT/WHY, never HOW or implementation detail.
  They earn their keep most on interfaces (`IUserRepository`, `ICreateService`,
  ...), less so on trivial constructors.

**Functional error handling.** The `Either<Failure, TResponse>` / `Match` contract
is in `AGENTS.md`; `Option<T>` marks absence. Value-object factories
(`Email.From`, `FirstName.From`, `UserId.From`, ...) still throw `ArgumentException`: they guard invariants that validation has already
enforced at the boundary, so a throw there is a programmer error, not an expected
failure. `IUnitOfWork.SaveChangesAsync` (the persistence adapter, `AppDbContext`)
returns `Task<Either<Failure, Unit>>` and translates storage rejections into domain
failures itself: a violation of the user email unique index becomes
`EmailAlreadyExistsError`, a `DbUpdateConcurrencyException` (row removed concurrently)
becomes `NotFoundError`, any other unique violation a detail-free
`UniqueConstraintViolationError`. Constraint names never leave Infrastructure. Other
`DbUpdateException`s are unmapped and reach the exception handler as a 500.

**Versioned routing convention.** All user-facing routes live under
`api/v{version:apiVersion}/...`, wired via `NewApiVersionSet()` /
`WithApiVersionSet()` on the endpoint group (see `UsersEndpoints`). A new endpoint
group must declare its own version set the same way; do not add unversioned routes
alongside it.
