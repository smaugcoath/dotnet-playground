---
paths:
  - "tests/**/*.cs"
---

# Testing guidelines

**Functional-first.** Prefer functional tests
(`Pocs.Aspire.ApiService.Tests.Functional`, driven through `AspireHostFixture` — a
real Aspire distributed app, real HTTP calls, real Postgres/Redis in
containers started by the Aspire host) and integration tests
(`Pocs.Aspire.Infrastructure.Tests.Integration`, real Postgres via Testcontainers)
over mocked unit tests. Reserve `Pocs.Aspire.Business.Tests.Unit` (NSubstitute
mocks) for paths a functional test structurally cannot reach — guard clauses like
constructor null-checks that DI never triggers in the running app.

**Assert the whole object.** When the thing under test returns a complex value
(`Option<T>`, `Either<Failure, T>`, an entity, a DTO), build a complete `expected`
value and assert the whole object (`actual.ShouldBeEquivalentTo(expected)`), so
every mapping is exercised — not just one flag (e.g. don't assert only
`result.IsNone.ShouldBeTrue()`; compare against `Option<User>.None`). Reserve
single-property/scalar asserts for genuinely simple values (an HTTP status code, a
count, a standalone boolean flag).

**AAA markers.** Keep bare `// Arrange` / `// Act` / `// Assert` markers — no
appended narration.

**Deterministic data.** Use fixed, explicit test data; no randomness, no
wall-clock or ambient time that makes a run's outcome vary.
