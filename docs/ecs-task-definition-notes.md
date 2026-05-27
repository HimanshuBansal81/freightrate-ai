# ECS Task Definition Notes

FreightRate AI should run each application service as its own ECS task definition and ECS service:

- Auth Service: one task definition and ECS service.
- Quote Service: one task definition and ECS service.
- AI Recommendation Service: one task definition and ECS service.

This keeps scaling, deployment, health checks, logs, and permissions independent for each service.

## Container Ports

- Auth Service uses container port `8080`.
- Quote Service uses container port `8080`.
- AI Recommendation Service supports `PORT` and defaults to `8000` locally. For ECS, set `PORT=8080` and configure
  the task/container/target group port as `8080`, or document and consistently use another configured port.

## Secrets

Inject sensitive values from AWS Secrets Manager through the ECS task definition `secrets` field:

- `ConnectionStrings__DefaultConnection`
- `Jwt__Secret`
- `Redis__ConnectionString`
- Optional AI provider keys such as `OPENAI_API_KEY`, `ANTHROPIC_API_KEY`, and `AZURE_OPENAI_API_KEY`

Use normal environment variables for non-secret settings such as `ASPNETCORE_ENVIRONMENT`, `ASPNETCORE_URLS`,
`Jwt__Issuer`, `Jwt__Audience`, `Jwt__ExpiryMinutes`, timeouts, `PORT`, `AI_PROVIDER`, and `AI_MODEL`.

## Logging

Configure the ECS `awslogs` log driver for each container so application logs go to CloudWatch Logs. Use separate log
groups or stream prefixes per service to keep troubleshooting simple:

- `/ecs/freightrate-ai/auth-service`
- `/ecs/freightrate-ai/quote-service`
- `/ecs/freightrate-ai/ai-recommendation-service`

## Load Balancer And Health Checks

Create one ALB target group per service. Configure target group health checks to call:

```text
/health
```

Use ALB listener rules for public path-based routing:

| Path pattern | Target group |
| --- | --- |
| `/auth/*` | Auth target group |
| `/quotes/*` | Quote target group |
| `/ai/*` | AI target group |

Public access should go through the Application Load Balancer. In a proper production-style setup, ECS tasks run in private subnets without direct public IP access.

## IAM

Use least-privilege IAM roles:

- Task execution role: ECR pull access and CloudWatch log write access.
- Task role: read access only to the Secrets Manager secrets required by that service.

Avoid sharing broad runtime permissions across all services unless they truly need the same AWS resources.
