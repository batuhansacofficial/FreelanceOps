# Database Schema

The initial EF Core `ApplicationDbContext` is configured for PostgreSQL and uses the default schema:

```text
freelance_ops
```

No migrations exist yet. The first meaningful migration should be added when the first domain entities are introduced, starting with auth and workspace tables.
