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

## Security Foundation

Authentication and workspace isolation are shared foundations for the business modules. Client, project, task, invoice, report, proposal, and notification flows use workspace-scoped access checks instead of relying on standalone CRUD endpoints.
