#!/usr/bin/env bash
set -euo pipefail

# Optional Cloud Run helper. Deploys images that already exist in Google Artifact Registry.
#
# Required environment variables:
#   GCP_PROJECT_ID
#   GCP_REGION                Example: asia-south1
#
# Required Secret Manager entries:
#   jwt-secret
#   auth-db-connection
#   quote-db-connection
#   redis-connection-string
#
# The AWS deployment path remains primary for this repository.

if [[ -z "${GCP_PROJECT_ID:-}" ]]; then
  echo "GCP_PROJECT_ID is required." >&2
  exit 1
fi

if [[ -z "${GCP_REGION:-}" ]]; then
  echo "GCP_REGION is required, for example: asia-south1." >&2
  exit 1
fi

REPOSITORY="${GCP_REGION}-docker.pkg.dev/${GCP_PROJECT_ID}/freightrate-ai"
AUTH_IMAGE="${REPOSITORY}/auth-service:latest"
QUOTE_IMAGE="${REPOSITORY}/quote-service:latest"
AI_IMAGE="${REPOSITORY}/ai-recommendation-service:latest"

gcloud run deploy freightrate-ai-service \
  --image "${AI_IMAGE}" \
  --region "${GCP_REGION}" \
  --platform managed \
  --allow-unauthenticated \
  --set-env-vars AI_PROVIDER=none,AI_TIMEOUT_SECONDS=5

AI_SERVICE_URL="$(gcloud run services describe freightrate-ai-service --region "${GCP_REGION}" --format='value(status.url)')"

gcloud run deploy freightrate-auth-service \
  --image "${AUTH_IMAGE}" \
  --region "${GCP_REGION}" \
  --platform managed \
  --allow-unauthenticated \
  --set-env-vars ASPNETCORE_ENVIRONMENT=Production,ASPNETCORE_URLS=http://+:8080,Jwt__Issuer=FreightRateAI.Auth,Jwt__Audience=FreightRateAI.Services,Jwt__ExpiryMinutes=60 \
  --set-secrets Jwt__Secret=jwt-secret:latest,ConnectionStrings__DefaultConnection=auth-db-connection:latest

gcloud run deploy freightrate-quote-service \
  --image "${QUOTE_IMAGE}" \
  --region "${GCP_REGION}" \
  --platform managed \
  --allow-unauthenticated \
  --set-env-vars ASPNETCORE_ENVIRONMENT=Production,ASPNETCORE_URLS=http://+:8080,Jwt__Issuer=FreightRateAI.Auth,Jwt__Audience=FreightRateAI.Services,AiService__BaseUrl="${AI_SERVICE_URL}",AiService__TimeoutSeconds=3 \
  --set-secrets Jwt__Secret=jwt-secret:latest,ConnectionStrings__DefaultConnection=quote-db-connection:latest,Redis__ConnectionString=redis-connection-string:latest
