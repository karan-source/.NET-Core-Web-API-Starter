# Production-Ready .NET Core Web API Starter

A reference ASP.NET Core Web API built the way production services actually need to be built: Clean Architecture layering, CQRS with MediatR, EF Core for writes, Dapper for reads, JWT authentication, structured logging, RFC 7807 error responses, and a real test suite.

Clone it, run one command, and you have a working API with OpenAPI docs and seeded data.

![CI](https://github.com/karan-source/Production-Ready-.NET-Core-Web-API-Starter/actions/workflows/ci.yml/badge.svg)

---

## Why this exists

Most .NET samples are either a single `Program.cs` with everything in it, or an enterprise template so abstract you can't find the business logic. This sits in between: enough structure to scale to a real team, little enough ceremony to still read it in one sitting.

Every piece here solves a problem that shows up on real production systems — slow queries, leaked exception details, untestable handlers, config drift between environments.

---

## Architecture

Dependencies point inwards. The domain knows nothing about the database, and the API knows nothing about how data is persisted.

**The dependency rule:** `Domain` has zero project references. `Application` defines interfaces (`IApplicationDbContext`, `IProductReadRepository`, `IJwtTokenService`); `Infrastructure` implements them. Swapping SQLite for SQL Server, or JWT for Entra ID, touches one layer.

### Request pipeline

```
HTTP request
  -> Rate limiter (100 req/min per IP)
  -> Authentication / Authorization
  -> Controller (thin - just dispatches)
  -> MediatR PerformanceBehaviour  (logs handlers slower than 500ms)
  -> MediatR ValidationBehaviour   (FluentValidation, fails before the handler runs)
  -> Handler (EF Core for writes, Dapper for reads)
  -> GlobalExceptionHandler -> RFC 7807 ProblemDetails
```

---

## Tech stack

| Concern | Choice |
|---|---|
| Framework | .NET 10 (LTS), ASP.NET Core |
| Architecture | Clean Architecture, CQRS, SOLID |
| Mediation | MediatR 12 with pipeline behaviours |
| Writes | Entity Framework Core 10 + migrations |
| Reads | Dapper (hand-tuned, paginated SQL) |
| Validation | FluentValidation |
| Auth | JWT bearer tokens |
| Logging | Serilog (structured, request logging) |
| Errors | RFC 7807 `ProblemDetails` via `IExceptionHandler` |
| Docs | OpenAPI + Scalar UI |
| Tests | xUnit, Moq, `WebApplicationFactory` |
| Delivery | Multi-stage Dockerfile, GitHub Actions CI |

---

## Getting started

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download)

```bash
git clone https://github.com/karan-source/Production-Ready-.NET-Core-Web-API-Starter.git
cd Production-Ready-.NET-Core-Web-API-Starter
dotnet run --project src/ProductionApi.Api
```

That's it — no database to install. The API creates a local SQLite file, applies migrations, and seeds a small product catalogue on first run.

- **API docs (Scalar):** http://localhost:5080/scalar
- **OpenAPI document:** http://localhost:5080/openapi/v1.json
- **Health check:** http://localhost:5080/health

Use [`src/ProductionApi.Api/ProductionApi.Api.http`](src/ProductionApi.Api/ProductionApi.Api.http) to fire requests straight from VS Code or Visual Studio.

---

## API

| Method | Route | Auth | Description |
|---|---|---|---|
| `GET` | `/api/products` | Anonymous | Paginated, searchable list (Dapper) |
| `GET` | `/api/products/{id}` | Anonymous | Single product (EF Core projection) |
| `POST` | `/api/products` | Bearer | Create |
| `PUT` | `/api/products/{id}` | Bearer | Update |
| `DELETE` | `/api/products/{id}` | Bearer | Delete |
| `POST` | `/api/auth/dev-token` | Anonymous | Issues a token — **development only** |
| `GET` | `/health` | Anonymous | Liveness probe |

### Example: paginated search

```http
GET /api/products?pageNumber=1&pageSize=10&search=desk
```

```json
{
  "items": [
    {
      "id": "0198f2c1-0000-7000-8000-000000000000",
      "name": "Standing Desk",
      "description": "Electric, dual motor.",
      "price": 599.00,
      "stockQuantity": 7,
      "isActive": true,
      "createdAtUtc": "2026-08-31T05:52:11.4820000+00:00"
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 1,
  "totalPages": 1,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

### Example: validation failure

Invalid input never reaches the handler. It comes back as `400` in RFC 7807 form:

```json
{
  "type": "https://httpstatuses.io/400",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "instance": "/api/products",
  "errors": {
    "Name": ["'Name' must not be empty."],
    "Price": ["'Price' must be greater than '0'."]
  }
}
```

---

## Testing

```bash
dotnet build ProductionApi.slnx
dotnet test ProductionApi.slnx --no-build
```

**21 tests, two layers:**

- **Unit tests** — validators, handlers, and the validation pipeline behaviour. Handlers run against in-memory SQLite rather than the EF in-memory provider, so relational behaviour is real.
- **Integration tests** — the full HTTP pipeline through `WebApplicationFactory`, including auth, validation, pagination, and 404/401 paths. Each run gets its own throwaway database file.

---

## Configuration and secrets

Non-secret defaults live in `appsettings.json`. **The JWT signing key is never committed.**

`appsettings.Development.json` contains an obviously-named local key so the project runs immediately after cloning. For anything deployed, supply the real key out of band:

```bash
# Local development
dotnet user-secrets set "Jwt:SigningKey" "<a-long-random-value>" --project src/ProductionApi.Api

# Or via environment variable
export Jwt__SigningKey="<a-long-random-value>"
```

The app **fails fast at startup** if the signing key is missing or shorter than 32 bytes — a misconfigured deployment stops immediately instead of silently issuing weak tokens.

---

## Security notes

- **No exception leakage.** `GlobalExceptionHandler` maps known exceptions to safe responses and returns a generic message for everything else; the stack trace goes to logs, never to the client.
- **Parameterised SQL everywhere.** The Dapper query uses bound parameters, and `LIKE` wildcards inside user input are escaped so a search term can't widen the result set.
- **The dev-token endpoint returns 404 outside Development.** A deployed instance cannot mint tokens.
- **Rate limiting** is on by default (fixed window, per IP).
- **The container runs as a non-root user.**

> The `/api/auth/dev-token` endpoint is a **stub for exercising protected routes locally**, not an identity system. Replace it with Microsoft Entra ID, Auth0, or ASP.NET Core Identity before real use.

---

## Design decisions worth knowing

**SQLite is the default provider** so the repo runs with zero setup. The write model is provider-agnostic, but the Dapper read query uses SQLite's `LIMIT ... OFFSET`. Moving to SQL Server means changing the provider registration in `Infrastructure/DependencyInjection.cs`, the connection factory, and that one paging clause to `OFFSET ... FETCH NEXT`.

**EF Core writes, Dapper reads.** Commands get change tracking and validation; queries get hand-written SQL with no tracking overhead. SQLite stores `Guid`, `decimal` and `DateTimeOffset` as text, so `SqliteTypeHandlers` teaches Dapper to read the columns EF Core writes.

**Migrations run at startup.** Convenient for a demo and small services. For multi-instance deployments, move this to a release pipeline step so instances don't race.

**Version 7 GUIDs** for primary keys — time-ordered, so index inserts stay sequential instead of fragmenting.

---

## Docker

```bash
docker build -t productionapi .
docker run -p 8080:8080 -e Jwt__SigningKey="<a-long-random-value>" productionapi
```

---

## Project structure

```
src/
  ProductionApi.Domain/          Entities, no dependencies
  ProductionApi.Application/     Commands, queries, validators, behaviours, interfaces
  ProductionApi.Infrastructure/  EF Core, migrations, Dapper, JWT
  ProductionApi.Api/             Controllers, exception handling, DI composition
tests/
  ProductionApi.Application.UnitTests/
  ProductionApi.Api.IntegrationTests/
```

---

## Working with migrations

```bash
dotnet tool restore

dotnet ef migrations add <Name> \
  --project src/ProductionApi.Infrastructure \
  --startup-project src/ProductionApi.Api \
  --output-dir Persistence/Migrations
```

---

## License

MIT — use it, fork it, ship it.