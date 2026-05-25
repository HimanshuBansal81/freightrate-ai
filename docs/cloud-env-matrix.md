# Cloud Environment Matrix

Use this matrix when mapping local `.env` values to Cloud Run environment variables and Secret Manager secrets. Placeholder examples are intentionally non-secret.

| Variable | Service | Local source | Cloud source | Secret or non-secret | Required | Example placeholder |
| --- | --- | --- | --- | --- | --- | --- |
| `ASPNETCORE_ENVIRONMENT` | Auth | `.env.example` / Docker Compose | Cloud Run env var | Non-secret | Yes | `Production` |
| `ASPNETCORE_URLS` | Auth | Docker Compose | Cloud Run env var | Non-secret | Yes | `http://+:8080` |
| `ConnectionStrings__DefaultConnection` | Auth | Docker Compose interpolation | Secret Manager `auth-db-connection` | Secret | Yes | `Host=<host>;Port=5432;Database=<auth-db>;Username=<user>;Password=<password>;SSL Mode=Require` |
| `Jwt__Secret` | Auth | `.env` from `.env.example` | Secret Manager `jwt-secret` | Secret | Yes | `<strong-jwt-secret>` |
| `Jwt__Issuer` | Auth | `.env.example` / Docker Compose | Cloud Run env var | Non-secret | Yes | `FreightRateAI.Auth` |
| `Jwt__Audience` | Auth | `.env.example` / Docker Compose | Cloud Run env var | Non-secret | Yes | `FreightRateAI.Services` |
| `Jwt__ExpiryMinutes` | Auth | `.env.example` / Docker Compose | Cloud Run env var | Non-secret | Yes | `60` |
| `ASPNETCORE_ENVIRONMENT` | Quote | `.env.example` / Docker Compose | Cloud Run env var | Non-secret | Yes | `Production` |
| `ASPNETCORE_URLS` | Quote | Docker Compose | Cloud Run env var | Non-secret | Yes | `http://+:8080` |
| `ConnectionStrings__DefaultConnection` | Quote | Docker Compose interpolation | Secret Manager `quote-db-connection` | Secret | Yes | `Host=<host>;Port=5432;Database=<quote-db>;Username=<user>;Password=<password>;SSL Mode=Require` |
| `Jwt__Secret` | Quote | `.env` from `.env.example` | Secret Manager `jwt-secret` | Secret | Yes | `<same-strong-jwt-secret-as-auth>` |
| `Jwt__Issuer` | Quote | `.env.example` / Docker Compose | Cloud Run env var | Non-secret | Yes | `FreightRateAI.Auth` |
| `Jwt__Audience` | Quote | `.env.example` / Docker Compose | Cloud Run env var | Non-secret | Yes | `FreightRateAI.Services` |
| `Redis__ConnectionString` | Quote | `.env.example` / Docker Compose | Secret Manager `redis-connection-string` | Secret | Yes | `<redis-host>:<port>,password=<password>,ssl=True` |
| `AiService__BaseUrl` | Quote | `.env.example` / Docker Compose | Cloud Run env var | Non-secret | Yes | `https://freightrate-ai-service-<hash>-<region>.a.run.app` |
| `AiService__TimeoutSeconds` | Quote | `.env.example` / Docker Compose | Cloud Run env var | Non-secret | Yes | `3` |
| `AI_PROVIDER` | AI | `.env.example` / Docker Compose | Cloud Run env var | Non-secret | Yes | `none` |
| `AI_MODEL` | AI | `.env.example` / Docker Compose | Cloud Run env var | Non-secret | No | `gpt-4o-mini` |
| `AI_TIMEOUT_SECONDS` | AI | `.env.example` / Docker Compose | Cloud Run env var | Non-secret | Yes | `5` |
| `OPENAI_API_KEY` | AI | Local `.env` only | Secret Manager `openai-api-key` | Secret | No | `<openai-api-key>` |
| `ANTHROPIC_API_KEY` | AI | Local `.env` only | Secret Manager `anthropic-api-key` | Secret | No | `<anthropic-api-key>` |
| `AZURE_OPENAI_ENDPOINT` | AI | Local `.env` only | Cloud Run env var or Secret Manager | Non-secret in most setups | No | `https://<resource>.openai.azure.com` |
| `AZURE_OPENAI_API_KEY` | AI | Local `.env` only | Secret Manager `azure-openai-api-key` | Secret | No | `<azure-openai-api-key>` |
