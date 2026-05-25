# AWS Environment Matrix

Use this matrix when mapping local `.env` values to ECS task definition environment variables and AWS Secrets Manager secrets. Placeholder examples are intentionally non-secret.

| Variable | Service | AWS source | Secret or non-secret | Required | Example placeholder |
| --- | --- | --- | --- | --- | --- |
| `ASPNETCORE_ENVIRONMENT` | Auth | ECS task environment variable | Non-secret | Yes | `Production` |
| `ASPNETCORE_URLS` | Auth | ECS task environment variable | Non-secret | Yes | `http://+:8080` |
| `ConnectionStrings__DefaultConnection` | Auth | Secrets Manager `freightrate-ai/auth-db-connection` | Secret | Yes | `Host=<rds-endpoint>;Port=5432;Database=<auth-db>;Username=<user>;Password=<password>;SSL Mode=Require` |
| `Jwt__Secret` | Auth | Secrets Manager `freightrate-ai/jwt-secret` | Secret | Yes | `<strong-jwt-secret>` |
| `Jwt__Issuer` | Auth | ECS task environment variable | Non-secret | Yes | `FreightRateAI.Auth` |
| `Jwt__Audience` | Auth | ECS task environment variable | Non-secret | Yes | `FreightRateAI.Services` |
| `Jwt__ExpiryMinutes` | Auth | ECS task environment variable | Non-secret | Yes | `60` |
| `ASPNETCORE_ENVIRONMENT` | Quote | ECS task environment variable | Non-secret | Yes | `Production` |
| `ASPNETCORE_URLS` | Quote | ECS task environment variable | Non-secret | Yes | `http://+:8080` |
| `ConnectionStrings__DefaultConnection` | Quote | Secrets Manager `freightrate-ai/quote-db-connection` | Secret | Yes | `Host=<rds-endpoint>;Port=5432;Database=<quote-db>;Username=<user>;Password=<password>;SSL Mode=Require` |
| `Jwt__Secret` | Quote | Secrets Manager `freightrate-ai/jwt-secret` | Secret | Yes | `<same-strong-jwt-secret-as-auth>` |
| `Jwt__Issuer` | Quote | ECS task environment variable | Non-secret | Yes | `FreightRateAI.Auth` |
| `Jwt__Audience` | Quote | ECS task environment variable | Non-secret | Yes | `FreightRateAI.Services` |
| `Redis__ConnectionString` | Quote | Secrets Manager `freightrate-ai/redis-connection-string` | Secret | Yes | `<redis-host>:<port>,password=<password>,ssl=True` |
| `AiService__BaseUrl` | Quote | ECS task environment variable | Non-secret | Yes | `http://<ai-service-discovery-name>:8080` |
| `AiService__TimeoutSeconds` | Quote | ECS task environment variable | Non-secret | Yes | `3` |
| `PORT` | AI | ECS task environment variable | Non-secret | Yes | `8080` |
| `AI_PROVIDER` | AI | ECS task environment variable | Non-secret | Yes | `none` |
| `AI_MODEL` | AI | ECS task environment variable | Non-secret | No | `gpt-4o-mini` |
| `AI_TIMEOUT_SECONDS` | AI | ECS task environment variable | Non-secret | Yes | `5` |
| `OPENAI_API_KEY` | AI | Secrets Manager `freightrate-ai/openai-api-key` | Secret | No | `<openai-api-key>` |
| `ANTHROPIC_API_KEY` | AI | Secrets Manager `freightrate-ai/anthropic-api-key` | Secret | No | `<anthropic-api-key>` |
| `AZURE_OPENAI_ENDPOINT` | AI | ECS task environment variable or Secrets Manager | Non-secret in most setups | No | `https://<resource>.openai.azure.com` |
| `AZURE_OPENAI_API_KEY` | AI | Secrets Manager `freightrate-ai/azure-openai-api-key` | Secret | No | `<azure-openai-api-key>` |

## Notes

- The same `Jwt__Secret`, `Jwt__Issuer`, and `Jwt__Audience` values must be used by Auth and Quote so Quote can validate Auth-issued tokens.
- Keep optional AI provider keys unset when `AI_PROVIDER=none`; the AI service will use its deterministic fallback explanation path.
- `AiService__BaseUrl` should point to the AI service through internal service discovery or a private/internal route. If routing through the ALB, use the ALB URL with the `/ai` prefix only if the application route behavior has been verified.
