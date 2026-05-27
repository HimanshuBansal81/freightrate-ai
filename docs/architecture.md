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

AWS ECS Fargate is the primary deployment target. In AWS, the local Nginx gateway maps to an Application Load Balancer with path-based listener rules:

- `/auth/*` to the Auth Service target group
- `/quotes/*` to the Quote Service target group
- `/ai/*` to the AI Recommendation Service target group

Supporting managed services:

- ECR stores container images.
- ECS Fargate runs service containers.
- RDS PostgreSQL stores relational data.
- ElastiCache Redis provides managed caching.
- AWS Secrets Manager stores secrets and connection strings.
- CloudWatch Logs collects service logs.

Google Cloud Run is documented separately as an optional alternative container deployment path, not the primary deployment architecture.

## Data Ownership

Auth Service owns users, roles, and password hashes. Quote Service owns carriers, zones, rate rules, quote requests,
and quote options. The AI service owns no source-of-truth business data.
