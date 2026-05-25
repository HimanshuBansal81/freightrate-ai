# Cloud Run Deployment Preparation

FreightRate AI is prepared for Google Cloud Run deployment, but this repository does not perform deployment automatically. Do not commit production `.env` files, database URLs, Redis URLs, JWT secrets, or AI provider keys.

## Prerequisites

- Google Cloud project
- Billing enabled
- `gcloud` CLI installed and authenticated
- Docker installed and authenticated where images are built
- Required APIs enabled:
  - Cloud Run
  - Artifact Registry
  - Secret Manager
  - Cloud Logging

```bash
export GCP_PROJECT_ID="your-gcp-project-id"
export GCP_REGION="asia-south1"
gcloud config set project "$GCP_PROJECT_ID"
gcloud services enable run.googleapis.com artifactregistry.googleapis.com secretmanager.googleapis.com logging.googleapis.com
```

`asia-south1` is a suggested region placeholder. Choose the region closest to your users and managed data services.

## Artifact Registry

Create a Docker repository:

```bash
gcloud artifacts repositories create freightrate-ai \
  --repository-format=docker \
  --location="$GCP_REGION" \
  --description="FreightRate AI Docker images"
```

Configure Docker authentication:

```bash
gcloud auth configure-docker "$GCP_REGION-docker.pkg.dev"
```

Image names:

```bash
AUTH_IMAGE="$GCP_REGION-docker.pkg.dev/$GCP_PROJECT_ID/freightrate-ai/auth-service:latest"
QUOTE_IMAGE="$GCP_REGION-docker.pkg.dev/$GCP_PROJECT_ID/freightrate-ai/quote-service:latest"
AI_IMAGE="$GCP_REGION-docker.pkg.dev/$GCP_PROJECT_ID/freightrate-ai/ai-recommendation-service:latest"
```

## Build And Push Images

Run from the repository root:

```bash
docker build -t "$AUTH_IMAGE" -f services/auth-service/Dockerfile .
docker push "$AUTH_IMAGE"

docker build -t "$QUOTE_IMAGE" -f services/quote-service/Dockerfile .
docker push "$QUOTE_IMAGE"

docker build -t "$AI_IMAGE" -f services/ai-recommendation-service/Dockerfile .
docker push "$AI_IMAGE"
```

The .NET services listen on `ASPNETCORE_URLS=http://+:8080`. The FastAPI service uses Cloud Run's `PORT` environment variable and defaults to `8000` for local Docker Compose.

## Secret Manager

Create secrets with real values outside Git. The commands below show names only; do not paste real values into documentation or commits.

Recommended secret names:

- `jwt-secret`
- `auth-db-connection`
- `quote-db-connection`
- `redis-connection-string`
- `openai-api-key` optional
- `anthropic-api-key` optional
- `azure-openai-api-key` optional

Example pattern:

```bash
printf "REPLACE_WITH_REAL_VALUE" | gcloud secrets create jwt-secret --data-file=-
```

For existing secrets, add a new version instead of recreating:

```bash
printf "REPLACE_WITH_REAL_VALUE" | gcloud secrets versions add jwt-secret --data-file=-
```

## Cloud Run Deploy Commands

Deploy the AI service first:

```bash
gcloud run deploy freightrate-ai-service \
  --image "$AI_IMAGE" \
  --region "$GCP_REGION" \
  --platform managed \
  --allow-unauthenticated \
  --set-env-vars AI_PROVIDER=none,AI_TIMEOUT_SECONDS=5
```

Copy the deployed AI service URL:

```bash
AI_SERVICE_URL="$(gcloud run services describe freightrate-ai-service --region "$GCP_REGION" --format='value(status.url)')"
```

Deploy the Auth service:

```bash
gcloud run deploy freightrate-auth-service \
  --image "$AUTH_IMAGE" \
  --region "$GCP_REGION" \
  --platform managed \
  --allow-unauthenticated \
  --set-env-vars ASPNETCORE_ENVIRONMENT=Production,ASPNETCORE_URLS=http://+:8080,Jwt__Issuer=FreightRateAI.Auth,Jwt__Audience=FreightRateAI.Services,Jwt__ExpiryMinutes=60 \
  --set-secrets Jwt__Secret=jwt-secret:latest,ConnectionStrings__DefaultConnection=auth-db-connection:latest
```

Deploy the Quote service after the AI service URL is known:

```bash
gcloud run deploy freightrate-quote-service \
  --image "$QUOTE_IMAGE" \
  --region "$GCP_REGION" \
  --platform managed \
  --allow-unauthenticated \
  --set-env-vars ASPNETCORE_ENVIRONMENT=Production,ASPNETCORE_URLS=http://+:8080,Jwt__Issuer=FreightRateAI.Auth,Jwt__Audience=FreightRateAI.Services,AiService__BaseUrl="$AI_SERVICE_URL",AiService__TimeoutSeconds=3 \
  --set-secrets Jwt__Secret=jwt-secret:latest,ConnectionStrings__DefaultConnection=quote-db-connection:latest,Redis__ConnectionString=redis-connection-string:latest
```

If you enable a real AI provider, set `AI_PROVIDER` and the relevant provider configuration on `freightrate-ai-service`, then map API keys from Secret Manager.

## Deployment Order

1. Create or choose PostgreSQL and Redis providers.
2. Store connection strings and secrets in Secret Manager.
3. Build and push all three images.
4. Deploy AI Recommendation Service.
5. Copy the AI service URL.
6. Deploy Auth Service.
7. Deploy Quote Service with `AiService__BaseUrl` set to the deployed AI service URL.

## Health Checks

Each service exposes `/health`:

```bash
curl "$AI_SERVICE_URL/health"
curl "$AUTH_SERVICE_URL/health"
curl "$QUOTE_SERVICE_URL/health"
```

For a quote smoke test, register or log in through the Auth service, capture the JWT, then call Quote Service:

```bash
curl -sS -X POST "$QUOTE_SERVICE_URL/api/quotes/compare" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "originPincode": "110001",
    "destinationPincode": "560001",
    "actualWeightKg": 8,
    "lengthCm": 40,
    "widthCm": 30,
    "heightCm": 25,
    "preference": "Balanced"
  }'
```

## Low-Cost External Providers

PostgreSQL can use Neon or Supabase:

```text
Host=<provider-host>;Port=5432;Database=<database>;Username=<user>;Password=<password>;SSL Mode=Require;Trust Server Certificate=true
```

Redis can use Upstash:

```text
<host>:<port>,password=<password>,ssl=True,abortConnect=False
```

Store both values in Secret Manager as `auth-db-connection`, `quote-db-connection`, and `redis-connection-string`.

## Full GCP Alternative

- Cloud SQL PostgreSQL for relational storage
- Memorystore Redis for cache
- Cloud Run service-to-service IAM where public access is not required
- API Gateway or HTTPS Load Balancer for a single public entry point
- Cloud Logging and Cloud Monitoring for observability

Private Cloud SQL and Memorystore networking may require a Serverless VPC Access connector and additional Cloud Run networking configuration.

## Optional Scripts

This repository includes helper scripts that use only environment variables and placeholders:

```bash
export GCP_PROJECT_ID="your-gcp-project-id"
export GCP_REGION="asia-south1"

./scripts/build-images.sh
./scripts/deploy-cloud-run.sh
```

The deploy script assumes Artifact Registry images are already pushed and required secrets already exist in Secret Manager.
