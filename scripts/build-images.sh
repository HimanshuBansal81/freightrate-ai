#!/usr/bin/env bash
set -euo pipefail

if [[ -z "${GCP_PROJECT_ID:-}" ]]; then
  echo "GCP_PROJECT_ID is required." >&2
  exit 1
fi

if [[ -z "${GCP_REGION:-}" ]]; then
  echo "GCP_REGION is required, for example: asia-south1." >&2
  exit 1
fi

REPOSITORY="${GCP_REGION}-docker.pkg.dev/${GCP_PROJECT_ID}/freightrate-ai"

docker build -t "${REPOSITORY}/auth-service:latest" -f services/auth-service/Dockerfile .
docker build -t "${REPOSITORY}/quote-service:latest" -f services/quote-service/Dockerfile .
docker build -t "${REPOSITORY}/ai-recommendation-service:latest" -f services/ai-recommendation-service/Dockerfile .

docker push "${REPOSITORY}/auth-service:latest"
docker push "${REPOSITORY}/quote-service:latest"
docker push "${REPOSITORY}/ai-recommendation-service:latest"
