# Architecture

FreightRate AI is a service-oriented backend MVP for multi-carrier freight quote comparison.

## Services

- Auth Service: .NET 8 API for registration, login, JWT generation, users, and roles.
- Quote Service: .NET 8 API for deterministic quote calculation, quote history, role-protected endpoints, Redis caching, and AI explanation integration.
- AI Recommendation Service: Python FastAPI API for concise recommendation explanations.
- PostgreSQL: source of truth for user, role, carrier, zone, rate-rule, quote request, and quote option data.
- Redis: cache for read-heavy quote reference data.
- Nginx: local reverse proxy for path-based routing.

## Why Microservices

The project separates responsibilities that would naturally scale and evolve independently:

- Auth can change identity rules without touching pricing.
- Quote calculation remains deterministic and testable.
- AI can be replaced, disabled, or upgraded without risking pricing correctness.
- Cache and database concerns stay behind Quote Service boundaries.

For an MVP this is intentionally small: Docker Compose locally, no message broker, no Kubernetes, and no distributed workflow engine.

## Local Routing With Nginx

Local Nginx listens on `localhost:8080` and routes by path:

- `/auth/` to Auth Service on internal port `8080`
- `/quotes/` to Quote Service on internal port `8080`
- `/ai/` to AI Recommendation Service on internal port `8000`

Direct published ports are available for debugging:

- Auth: `5001`
- Quote: `5002`
- AI: `8000`

## Cloud Routing

For a low-cost cloud deployment, each service can run on Cloud Run and expose HTTPS endpoints directly or through an API Gateway.

For a fuller GCP architecture:

- API Gateway or HTTPS Load Balancer routes public traffic.
- Cloud Run hosts service containers.
- Cloud SQL or external managed PostgreSQL stores relational data.
- Memorystore or Upstash provides Redis.
- Secret Manager stores secrets and connection strings.
- Cloud Logging and Monitoring collect observability data.

## Data Ownership

Auth Service owns users, roles, and password hashes. Quote Service owns carriers, zones, rate rules, quote requests, and quote options. The AI service owns no source-of-truth business data.
