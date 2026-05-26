#!/usr/bin/env bash
set -euo pipefail

BASE_URL="${BASE_URL:-http://localhost:8080}"
OUTPUT_DIR="docs/demo-outputs"
PASSWORD="Passw0rd!"
TIMESTAMP="$(date +%Y%m%d%H%M%S)"
EMAIL="demo.shipper+${TIMESTAMP}@example.com"
FULL_NAME="Demo Shipper ${TIMESTAMP}"

require_command() {
  local command_name="$1"

  if ! command -v "$command_name" >/dev/null 2>&1; then
    echo "Error: '$command_name' is required but was not found." >&2
    echo "Install '$command_name' and rerun this script." >&2
    exit 1
  fi
}

request() {
  local method="$1"
  local url="$2"
  local output_file="$3"
  local data="${4:-}"
  local token="${5:-}"
  local body_file status

  body_file="$(mktemp)"

  if [[ -n "$data" && -n "$token" ]]; then
    status="$(curl -sS -o "$body_file" -w "%{http_code}" -X "$method" "$url" \
      -H "Content-Type: application/json" \
      -H "Authorization: Bearer ${token}" \
      -d "$data")"
  elif [[ -n "$data" ]]; then
    status="$(curl -sS -o "$body_file" -w "%{http_code}" -X "$method" "$url" \
      -H "Content-Type: application/json" \
      -d "$data")"
  elif [[ -n "$token" ]]; then
    status="$(curl -sS -o "$body_file" -w "%{http_code}" -X "$method" "$url" \
      -H "Authorization: Bearer ${token}")"
  else
    status="$(curl -sS -o "$body_file" -w "%{http_code}" -X "$method" "$url")"
  fi

  if [[ "$status" -lt 200 || "$status" -ge 300 ]]; then
    echo "Error: ${method} ${url} failed with HTTP ${status}." >&2
    cat "$body_file" >&2
    rm -f "$body_file"
    exit 1
  fi

  jq . "$body_file" > "$output_file"
  rm -f "$body_file"
}

redact_tokens() {
  local input_file="$1"
  local output_file="$2"

  jq 'walk(if type == "object" and has("token") then .token = "<REDACTED>" else . end)' \
    "$input_file" > "$output_file"
}

require_command curl
require_command jq

mkdir -p "$OUTPUT_DIR"

tmp_dir="$(mktemp -d)"
trap 'rm -rf "$tmp_dir"' EXIT

echo "Generating sanitized demo outputs from ${BASE_URL}"

request GET "${BASE_URL}/auth/health" "${OUTPUT_DIR}/health-auth.json"
echo "Saved ${OUTPUT_DIR}/health-auth.json"

request GET "${BASE_URL}/quotes/health" "${OUTPUT_DIR}/health-quote.json"
echo "Saved ${OUTPUT_DIR}/health-quote.json"

request GET "${BASE_URL}/ai/health" "${OUTPUT_DIR}/health-ai.json"
echo "Saved ${OUTPUT_DIR}/health-ai.json"

register_payload="$(jq -n \
  --arg fullName "$FULL_NAME" \
  --arg email "$EMAIL" \
  --arg password "$PASSWORD" \
  '{fullName: $fullName, email: $email, password: $password, role: "Shipper"}')"

request POST "${BASE_URL}/auth/api/auth/register" "${tmp_dir}/register-response.json" "$register_payload"
redact_tokens "${tmp_dir}/register-response.json" "${OUTPUT_DIR}/register-response.json"
echo "Saved ${OUTPUT_DIR}/register-response.json"

login_payload="$(jq -n \
  --arg email "$EMAIL" \
  --arg password "$PASSWORD" \
  '{email: $email, password: $password}')"

request POST "${BASE_URL}/auth/api/auth/login" "${tmp_dir}/login-response.json" "$login_payload"
token="$(jq -r '.token // empty' "${tmp_dir}/login-response.json")"

if [[ -z "$token" ]]; then
  echo "Error: login response did not include a token." >&2
  exit 1
fi

redact_tokens "${tmp_dir}/login-response.json" "${OUTPUT_DIR}/login-response-redacted.json"
echo "Saved ${OUTPUT_DIR}/login-response-redacted.json"

quote_payload='{
  "originPincode": "110001",
  "destinationPincode": "560001",
  "actualWeightKg": 8,
  "lengthCm": 40,
  "widthCm": 30,
  "heightCm": 25,
  "preference": "Balanced"
}'

request POST "${BASE_URL}/quotes/api/quotes/compare" "${OUTPUT_DIR}/quote-compare-response.json" "$quote_payload" "$token"
echo "Saved ${OUTPUT_DIR}/quote-compare-response.json"

request GET "${BASE_URL}/quotes/api/quotes/history" "${OUTPUT_DIR}/quote-history-response.json" "" "$token"
echo "Saved ${OUTPUT_DIR}/quote-history-response.json"

ai_payload='{
  "preference": "Balanced",
  "recommendedCarrier": "Xpressbees",
  "cheapestCarrier": "Xpressbees",
  "fastestCarrier": "BlueDart",
  "options": [
    {
      "carrier": "Xpressbees",
      "amount": 166.14,
      "etaDays": 5
    },
    {
      "carrier": "Delhivery",
      "amount": 192.95,
      "etaDays": 4
    },
    {
      "carrier": "BlueDart",
      "amount": 284.97,
      "etaDays": 2
    }
  ]
}'

request POST "${BASE_URL}/ai/api/recommendations/explain" "${OUTPUT_DIR}/ai-explanation-response.json" "$ai_payload"
echo "Saved ${OUTPUT_DIR}/ai-explanation-response.json"

echo "Done. Review ${OUTPUT_DIR} before committing generated outputs."

