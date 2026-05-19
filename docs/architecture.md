# Architecture

FreightRate AI starts as a small service-oriented monorepo:

- Auth Service: .NET 8 Web API for authentication workflows.
- Quote Service: .NET 8 Web API for quote, carrier, zone, surcharge, GST, and ETA workflows.
- AI Recommendation Service: FastAPI service for explanation generation.
- PostgreSQL: relational persistence.
- Redis: caching layer.
- Nginx: local path-based reverse proxy.

Business logic is intentionally deferred until the service contracts and data model are designed.
