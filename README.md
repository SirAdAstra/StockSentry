# StockSentry

![Status](https://img.shields.io/badge/status-work%20in%20progress-yellow)
![.NET](https://img.shields.io/badge/.NET-10-512BD4)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1)
![Kafka](https://img.shields.io/badge/Kafka-KRaft-231F20)

> 🚧 This project is under active development. Features described below are being implemented incrementally — see [Roadmap](#roadmap) for current progress.

## About

**StockSentry** is a warehouse inventory management API with automatic low-stock alerting, built as a portfolio project to demonstrate backend development skills with **ASP.NET Core Web API** and **EF Core** — using an event-driven architecture powered by **Apache Kafka**.

The core idea: every stock movement (in/out/adjustment) recalculates the current quantity for a product at a warehouse. When quantity drops below a configured reorder threshold, the API publishes an event to Kafka. A separate, decoupled worker service consumes that event, persists a low-stock alert, and logs it — simulating a notification to the purchasing department without depending on real email delivery.

## Tech Stack

- **.NET 10**, ASP.NET Core Web API (controller-based)
- **EF Core** + **PostgreSQL**
- **Apache Kafka** (KRaft mode, no Zookeeper) — producer in the API, consumer in a separate worker project
- **JWT** authentication with role-based authorization (Admin / Manager / Viewer)
- **FluentValidation**, **Serilog**
- **xUnit** (unit + integration tests)
- **Docker Compose** for local orchestration (API, Worker, PostgreSQL, Kafka, pgAdmin, Kafka UI)
- **GitHub Actions** for CI (build + tests on every push)

## Architecture

The solution follows a layered architecture, with an intentionally separate consumer service:

```
StockSentry/
├── src/
│   ├── Api/               # Controllers, middleware, Program.cs
│   ├── Application/       # DTOs, service interfaces, validation
│   ├── Domain/             # Entities, enums, business rules
│   ├── Infrastructure/    # DbContext, repositories, Kafka producer
│   └── AlertWorker/       # Standalone Kafka consumer worker
├── tests/
│   ├── UnitTests/
│   └── IntegrationTests/
└── docker-compose.yml
```

Splitting `AlertWorker` into its own process is a deliberate choice — it demonstrates designing a decoupled service rather than handling everything inline in the API.

## Planned Features

- **Inventory tracking**: products, categories, suppliers, warehouses, and per-warehouse stock levels
- **Stock movements**: recording in/out/adjustment operations with full history and audit trail
- **Automatic low-stock alerts**: threshold-based, event-driven via Kafka, with manual resolution
- **Role-based access control**: Admin / Manager / Viewer via JWT
- **Filtering & pagination** on all list endpoints
- **Swagger UI** for interactive API exploration
- **Centralized error handling**, request validation, and structured logging
- **Dockerized** local dev environment with admin tooling (pgAdmin, Kafka UI)

## Roadmap

- [x] Project & solution structure
- [x] Docker Compose skeleton (PostgreSQL + Kafka, KRaft mode)
- [x] Database connection & first migration
- [ ] Domain entities & DbContext configuration
- [ ] Core CRUD API (products, categories, suppliers, warehouses)
- [ ] Stock movement logic & threshold checks
- [ ] Kafka producer/consumer & alert persistence
- [ ] JWT authentication & role-based authorization
- [ ] Cross-cutting concerns (error handling, validation, logging)
- [ ] Test coverage (unit + integration)
- [ ] Full Docker setup & seed data
- [ ] CI pipeline & documentation polish

## Running Locally

```bash
docker compose up -d
```

Then apply migrations and run the API — detailed setup instructions will be added once the project reaches a runnable state.

---

*This is a learning/portfolio project — feedback and suggestions are welcome.*
