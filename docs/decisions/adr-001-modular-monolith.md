# ADR-001: Use Modular Monolith Architecture

## Status

Accepted

## Context

The project needs to demonstrate real-world backend design without adding distributed system complexity too early.

## Decision

Use a modular monolith architecture. Each business capability will be grouped into its own module, while the application remains a single deployable unit.

## Consequences

### Positive

- Easier local development
- Easier debugging
- Lower operational complexity
- Clearer module boundaries

### Negative

- Modules are not independently deployable
- Boundaries require discipline because everything runs in one process
