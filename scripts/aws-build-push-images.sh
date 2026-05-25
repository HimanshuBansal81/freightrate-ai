#!/usr/bin/env bash
set -euo pipefail

missing=0

if [[ -z "${AWS_REGION:-}" ]]; then
  echo "AWS_REGION is required, for example: ap-south-1." >&2
  missing=1
fi

if [[ -z "${AWS_ACCOUNT_ID:-}" ]]; then
  echo "AWS_ACCOUNT_ID is required." >&2
  missing=1
fi

if [[ -z "${ECR_REPOSITORY_PREFIX:-}" && -z "${ECR_NAMESPACE:-}" ]]; then
  echo "ECR_REPOSITORY_PREFIX or ECR_NAMESPACE is required, for example: freightrate-ai." >&2
  missing=1
fi

if [[ "$missing" -ne 0 ]]; then
  exit 1
fi

ECR_PREFIX="${ECR_REPOSITORY_PREFIX:-${ECR_NAMESPACE}}"
ECR_REGISTRY="${AWS_ACCOUNT_ID}.dkr.ecr.${AWS_REGION}.amazonaws.com"

aws ecr get-login-password --region "${AWS_REGION}" \
  | docker login --username AWS --password-stdin "${ECR_REGISTRY}"

docker build -t "${ECR_REGISTRY}/${ECR_PREFIX}/auth-service:latest" -f services/auth-service/Dockerfile .
docker build -t "${ECR_REGISTRY}/${ECR_PREFIX}/quote-service:latest" -f services/quote-service/Dockerfile .
docker build -t "${ECR_REGISTRY}/${ECR_PREFIX}/ai-recommendation-service:latest" -f services/ai-recommendation-service/Dockerfile .

docker push "${ECR_REGISTRY}/${ECR_PREFIX}/auth-service:latest"
docker push "${ECR_REGISTRY}/${ECR_PREFIX}/quote-service:latest"
docker push "${ECR_REGISTRY}/${ECR_PREFIX}/ai-recommendation-service:latest"
