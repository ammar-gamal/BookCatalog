# BookCatalog

> 📖 **Note:** For deep architectural decisions, layer breakdowns, and evolutionary details, refer to the [Design Document](docs/DESIGN.md).

## Table of Contents

- [Overview](#overview)
- [Schema Design](#schema-design)
- [Setup](#setup)
- [Testing](#testing)

---

## Overview

The **Book Catalog Platform** provides a complete backend system to manage library inventory and operations:

- **Authors**: Stores biographical records for authors who write the books in the catalog.
- **Conceptual Books**: Search, filter (genre, price, publication date), sort, and paginate book titles with normalized ISBNs.
- **Physical Inventory**: Distinguish conceptual book titles from real physical copies on shelves via unique barcodes.
- **User Accounts**: Register members who borrow and return books.
- **Lending Lifecycle**: Manage borrowing and returns, track loan histories, and enforce business rules (prevent double-borrowing of active copies via database constraints).
- **Production readiness**: Built with Serilog structured logging synced to Seq, SQL Server connection retries, health check endpoints, and graceful shutdown.

### Tech Stack

- **Framework:** ASP.NET Core Web API (.NET 10)
- **Database & ORM:** Microsoft SQL Server 2025 & Entity Framework Core 10
- **Logging:** Serilog + Seq
- **Containerization:** Docker & Docker Compose
- **Testing:** xUnit v3, FluentAssertions, FakeTimeProvider, Moq, MockQueryable.Moq, Testcontainers (SQL Server), Respawn.

---

## Schema Design

### Entity-Relationship Diagram (ERD)

![Entity-Relationship Diagram](docs/ERD.png)

---

### Physical Database Schema

![Physical Database Schema](docs/Schema.jpg)

---

## Setup

You can run the entire platform with one command using **Docker Compose**.

### Prerequisites

- [Docker](https://docs.docker.com/get-docker/) & [Docker Compose](https://docs.docker.com/compose/) installed and running

### Run with Docker Compose

1. **Clone the repository:**

   ```bash
   git clone https://github.com/ammar-gamal/BookCatalog.git
   cd BookCatalog
   ```

2. **Configure Environment Variables:**
   Copy `.env.example` to create your local `.env` file:

     ```bash
     cp .env.example .env
     ```

   > **Note:** Customize values in `.env` if desired. The defaults are preconfigured to match `docker-compose.yml`.

3. **Start the services:**

   ```bash
   docker compose up --build
   ```

   > **Note:** EF Core migrations run automatically on startup in Development mode, so the database will be created and migrated without manual intervention.

4. **Access the services:**
   - **Swagger UI (Interactive API Docs):** [http://localhost:5000/swagger](http://localhost:5000/swagger)
   - **Seq Log Viewer:** [http://localhost:3000](http://localhost:3000)
   - **Health Checks:**
     - Liveness: `http://localhost:5000/healthz/live`
     - Readiness: `http://localhost:5000/healthz/ready`

5. **Stop the services:**

   ```bash
   docker compose down
   ```

---

## Testing

**Prerequisites:**

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://docs.docker.com/get-started/get-docker/) (required for integration tests, as Testcontainers starts a real SQL Server container)

The solution includes both isolated unit tests and end-to-end integration tests.

### 1. Run All Tests

```bash
dotnet test
```

### 2. Run Unit Tests Only

Unit tests run completely in-memory with mocked dependencies and do not require Docker or a database:

```bash
dotnet test --project tests/BookCatalog.UnitTests
```

### 3. Run Integration Tests Only

Integration tests execute real HTTP endpoints against a real Microsoft SQL Server instance using **Testcontainers** and clean up state using **Respawn**. Ensure Docker Desktop is running before executing:

```bash
dotnet test --project tests/BookCatalog.IntegrationTests
```
