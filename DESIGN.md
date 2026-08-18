# Book Catalog Platform — Design Document

### What I Built

A REST API for a book catalog, built with ASP.NET Core (.NET 10). It contain the full CRUD operations for books.

Endpoints:

| Method | Route             | Description              |
| ------ | ----------------- | ------------------------ |
| GET    | `/api/book`       | Get all books             |
| GET    | `/api/book/{id}`  | Get a single book by ID   |
| POST   | `/api/book`       | Create a new book         |
| PUT    | `/api/book/{id}`  | Update an existing book   |
| DELETE | `/api/book/{id}`  | Delete a book             |

A `Book` has: `Id`, `Title`, `Author`, `Isbn`, `NormalizedIsbn`, `Price`, `Description`, `PublicationYear`, and `Genre` (an enum: Fiction, NonFiction, Science, History, General).

---

### How the Solution Is Structured

The project lives in a single ASP.NET Core project (`BookCatalog.API`), but organized in deliberate layers:

```
BookCatalog.API/
├── Controllers/       → HTTP boundary (thin, no business logic)
├── Services/          → Business rules
├── Repositories/      → Data access abstraction (interfaces + in-memory impl)
├── Dtos/              → What the API receives and returns
├── Entities/          → Domain models (internal to the app)
├── Utilities/         → Result pattern, Error types, ISBN normalizer
├── Exceptions/        → Global exception handler
└── ExtensionMethods/  → Mapping and ModelState helpers
```

Every layer communicates through **interfaces**, not concrete types. The controllers only know `IBookService`. The services only know `IBookRepository`. This means the data layer can be swapped (in-memory → EF Core / SQL) without touching services or controllers.

---

### Decisions I Made and Why

#### 1. Layered Single-Project Architecture over Clean Architecture Overengineering

I was initially considering using Clean Architecture with CQRS (an approach I used in a previous project, [Chattr](https://github.com/ammar-gamal/Chattr)), but i felt this would be overengineering and introduce unnecessary complexity.

Instead, I chose a middle ground: a layered single project architecture so that migrating to full Clean Architecture later (if complexity grows) is straightforward. The interfaces are already in place, and the layers are already separated. If I ever need to extract `BookCatalog.Domain` or `BookCatalog.Infrastructure` into their own class libraries, the code is ready for it.

The core principle I kept from Clean Architecture is that business logic must not know or care about infrastructure concerns. Services just communicate through defined interfaces (`IBookRepository`, `IBaseRepository<T>`), ensuring zero coupling between business logic and infrastructure details.

This approach also gave me an advantage which is keeping boundaries organized via namespaces and folders within a single project delivers most of the benefits of separation of concerns and testability, while avoiding the overhead of managing multiple project files and references.

#### 2. Generic Repository Pattern (`IBaseRepository<TEntity>`)

A generic base repository interface and its implementation (`IBaseRepository<TEntity>` and `InMemoryBaseRepository<TEntity>`)  were introduced to handle standard data operations uniformly.

The reasoning:

- **Data Access Abstraction**: The repository provides an abstraction over how data is stored and retrieved, keeping the service layer independent from the underlying data-access implementation. This makes it easier to replace the current in-memory storage with a database implementation later without changing the business logic.
- **DRY & Eliminating Boilerplate**: Standard CRUD operations (`AddAsync`, `GetByIdAsync`, `GetAll`, `UpdateAsync`, `DeleteAsync`) are identical for any entity inheriting from `Entity`. Implementing them once in a generic base avoids alot of duplications.
- **Focused Specific Repositories**: Specific interfaces like `IBookRepository` inherit the standard operations from `IBaseRepository` and only focus on declaring domain-specific queries (such as `GetBookIdByIsbnAsync`).

#### 3. `ConcurrentDictionary` for in-memory storage

Because we are storing our data in memory so the repository is registered as a **Singleton** — one shared instance for the lifetime of the app, which means all HTTP requests hit the same instance. Using a plain `Dictionary<TKey,TValue>` here would introduce a race condition and become not thread-safe.

`ConcurrentDictionary` handles concurrent reads and writes safely without requiring manual locks and ID generation is handled with `Interlocked.Increment`, which is also thread-safe.

#### 4. Sequential integer `Id` instead of `Guid`

Entities use an integer `Id` (`int`) instead of a `Guid`.

The reasoning is proactive database design for Week 3:

- In relational databases (e.g., SQL Server), the primary key typically acts as the **clustered index** which has it's own physical storage and the table is sorted based on it.
- Integer IDs are naturally appended at the end of the index, minimizing insertion overhead (unlike random Guids, which sometimes can cause page splits).
- Integer IDs are significantly smaller (4 bytes vs. 16 bytes for a Guid). This reduces the size of the index, allowing more entries to fit per page and improving the read/write performance.
- Integer IDs produce cleaner, more human-readable REST URLs (e.g., `/api/book/1` vs `/api/book/3fa85f64-5717-4562-b3fc-2c963f66afa6`).

### 5. Storing both Isbn and NormalizedIsbn

ISBNs can be submitted in different formats. The NormalizedIsbn field (while is uppercase version of Isbn) is what uniqueness checks run against. The original Isbn is preserved to return back to the client exactly as they submitted it.

#### 6. Returning `IQueryable<T>` from Repository

The repository exposes `IQueryable<T>` via `GetAll()` instead of returning a plain `List<T>` or `IEnumerable<T>`. This choice is meant to improve performance once we move to database.

The reasoning:

- **Direct Projection**: The service layer can project data directly into DTOs (`.Select(...)`) instead of loading entities into memory and mapping them to DTOs.
- **Deffered Exceution**: The query is not executed immediately. The service layer can compose additional operations such as filtering, sorting, and pagination (.Where(), .OrderBy(), .Skip(), .Take()) before the query is finally executed.
- **Seamless Database Migration**: When transitioning to EF Core in Week 3, `IQueryable` translates LINQ expressions directly into efficient SQL queries evaluated on the database server.

#### 7. `TimeProvider` injected as a dependency

Rather than calling `DateTime.UtcNow` directly, `TimeProvider.System` is injected. This makes time controllable in tests — a unit test can pass a fake `TimeProvider` that returns a fixed date, making publication-year validation tests deterministic.

#### 8. `Result<T>` pattern instead of exceptions

Services return `Result<T>` objects rather than throwing exceptions for business-level failures like "book not found" or "ISBN already exists".

The reasoning: **exceptions are for unexpected failures, not expected outcomes**. A missing book is not an unexpected crash — it is a normal, predictable case. Throwing and catching exceptions for this wastes a call stack allocation.

With `Result<T>`, the controller always knows whether the operation succeeded or not, and maps the error to the right HTTP status code through `AppController.HandleError()`. This keeps error handling explicit, centralized, and readable.

Unhandled, truly unexpected exceptions (bugs, infrastructure failures) are caught by `GlobalExceptionHandler`, which logs them as critical and returns a safe `500 Internal Server Error` response — without leaking stack traces to the client.

#### 9. Validation in two places — for different reasons

- **DataAnnotations on DTOs** (`[Required]`, `[MaxLength]`, `[Range]`) catch structurally invalid requests before they ever reach the service layer. ASP.NET Core's `[ApiController]` attribute rejects these automatically with a `400 Bad Request`.
- **Business rules in the service** (ISBN uniqueness, publication date must be in the past) live in `BookService`.

#### 10. `AppController` base class

All controllers inherit `AppController`, which provides a single `HandleError(Result result)` method. This method translates an `Error` type into the correct `ProblemDetails` HTTP response (`404`, `409`, `400`, etc.). Without this, every controller action would repeat the same switch statement.

#### 11. Consistent error response format

All errors — validation errors, not-found, conflicts, unhandled exceptions — return `ProblemDetails` (RFC 7807). Every error response includes `requestId` and `traceId` so errors can be correlated with logs.

---

### What I Would Improve With More Time

**Pagination on GET /books.** Right now the endpoint returns every book in memory. As the catalog grows, this becomes a problem. Adding `page` and `pageSize` query parameters with a total count in the response would more suitable rather than returning all books.

---
