# Architecture

FreelanceOps starts as a modular monolith. It remains a single deployable backend while keeping business capabilities separated by module.

## Layers

```text
FreelanceOps.Api
FreelanceOps.Application
FreelanceOps.Domain
FreelanceOps.Infrastructure
FreelanceOps.Worker
```

## Dependency Rules

- `Domain` contains business rules and has no project dependencies.
- `Application` coordinates use cases and depends on `Domain`.
- `Infrastructure` implements persistence and technical integrations.
- `Api` exposes HTTP endpoints and composes application services.
- `Worker` hosts background jobs and reuses application/infrastructure registrations.

## First Milestone

Auth and workspace isolation must be implemented before client, project, task, invoice, or report modules. Otherwise the project becomes ordinary CRUD and tenant isolation has to be retrofitted later.
