# GCP Deployment

This repository is ready for container-based deployment planning. No Kubernetes, Terraform, or CI/CD automation is included yet.

## Practical Low-Cost Deployment

- Cloud Run for Auth Service, Quote Service, and AI Recommendation Service.
- Artifact Registry for container images.
- Neon or Supabase PostgreSQL for managed relational storage.
- Upstash Redis for managed cache.
- Secret Manager for JWT secret, database URLs, Redis URL, and optional AI keys.
- Cloud Logging for service logs.

This setup keeps fixed monthly cost low while preserving the same service boundaries as local Docker Compose.

## Full GCP Deployment

- Cloud Run for services.
- Artifact Registry for images.
- Cloud SQL for PostgreSQL.
- Memorystore for Redis.
- Secret Manager for secrets.
- API Gateway or HTTPS Load Balancer for public routing.
- Cloud Logging and Cloud Monitoring for observability.

## Environment Variables

Auth Service:

- `ConnectionStrings__DefaultConnection`
- `Jwt__Secret`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__ExpiryMinutes`

Quote Service:

- `ConnectionStrings__DefaultConnection`
- `Jwt__Secret`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Redis__ConnectionString`
- `AiService__BaseUrl`
- `AiService__TimeoutSeconds`

AI Recommendation Service:

- `AI_PROVIDER`
- `AI_MODEL`
- `AI_TIMEOUT_SECONDS`
- optional provider keys such as `OPENAI_API_KEY`

## Production Security Notes

- Store secrets in Secret Manager, not in source control.
- Use strong JWT secrets for MVP deployments.
- Prefer asymmetric JWT signing for production-grade multi-service validation.
- Restrict database and Redis access to service identities or private networking where possible.
