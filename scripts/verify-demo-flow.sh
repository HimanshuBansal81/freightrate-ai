#!/usr/bin/env bash
set -euo pipefail

BASE_URL="${BASE_URL:-http://localhost:8080}"
PASSWORD="Passw0rd!"
TIMESTAMP="$(date +%Y%m%d%H%M%S)"
EMAIL="demo.shipper+${TIMESTAMP}@example.com"
FULL_NAME="Demo Shipper ${TIMESTAMP}"

require_command() {
  local command_name="$1"

  if ! command -v "$command_name" >/dev/null 2>&1; then
    echo "FAIL: '$command_name' is required but was not found." >&2
    echo "Install '$command_name' and rerun this script." >&2
    exit 1
  fi
}

request_json() {
  local method="$1"
  local url="$2"
  local data="${3:-}"
  local token="${4:-}"
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
    echo "FAIL: ${method} ${url} returned HTTP ${status}." >&2
    cat "$body_file" >&2
    rm -f "$body_file"
    exit 1
  fi

  jq . "$body_file"
  rm -f "$body_file"
}

redact_tokens() {
  jq 'walk(if type == "object" and has("token") then .token = "<REDACTED>" else . end)'
}

print_step() {
  echo
  echo "==> $1"
}

require_command curl
require_command jq

echo "Verifying demo flow against ${BASE_URL}"
echo "This script requires the local Docker Compose services to be running."

print_step "Auth health"
request_json GET "${BASE_URL}/auth/health"
echo "PASS: Auth health endpoint is available."

print_step "Quote health"
request_json GET "${BASE_URL}/quotes/health"
echo "PASS: Quote health endpoint is available."

print_step "AI health"
request_json GET "${BASE_URL}/ai/health"
echo "PASS: AI health endpoint is available."

register_payload="$(jq -n \
  --arg fullName "$FULL_NAME" \
  --arg email "$EMAIL" \
  --arg password "$PASSWORD" \
  '{fullName: $fullName, email: $email, password: $password, role: "Shipper"}')"

print_step "Register demo user"
register_response="$(request_json POST "${BASE_URL}/auth/api/auth/register" "$register_payload")"
printf '%s\n' "$register_response" | redact_tokens
echo "PASS: Registered ${EMAIL}."

login_payload="$(jq -n \
  --arg email "$EMAIL" \
  --arg password "$PASSWORD" \
  '{email: $email, password: $password}')"

print_step "Login demo user"
login_response="$(request_json POST "${BASE_URL}/auth/api/auth/login" "$login_payload")"
token="$(printf '%s\n' "$login_response" | jq -r '.token // empty')"

if [[ -z "$token" ]]; then
  echo "FAIL: login response did not include a token." >&2
  exit 1
fi

printf '%s\n' "$login_response" | redact_tokens
echo "PASS: Login returned a JWT token. Token redacted in console output."

quote_payload='{
  "originPincode": "110001",
  "destinationPincode": "560001",
  "actualWeightKg": 8,
  "lengthCm": 40,
  "widthCm": 30,
  "heightCm": 25,
  "preference": "Balanced"
}'

print_step "Compare quote"
quote_response="$(request_json POST "${BASE_URL}/quotes/api/quotes/compare" "$quote_payload" "$token")"
printf '%s\n' "$quote_response"
recommended_carrier="$(printf '%s\n' "$quote_response" | jq -r '.recommendedCarrier // empty')"

if [[ "$recommended_carrier" != "Xpressbees" ]]; then
  echo "FAIL: expected recommendedCarrier to be Xpressbees, got '${recommended_carrier}'." >&2
  exit 1
fi

echo "PASS: Quote compare returned expected Balanced recommendation."

print_step "Quote history"
history_response="$(request_json GET "${BASE_URL}/quotes/api/quotes/history" "" "$token")"
printf '%s\n' "$history_response"
history_count="$(printf '%s\n' "$history_response" | jq 'length')"

if [[ "$history_count" -lt 1 ]]; then
  echo "FAIL: quote history did not include the new quote." >&2
  exit 1
fi

echo "PASS: Quote history returned at least one quote."

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

print_step "AI explanation"
ai_response="$(request_json POST "${BASE_URL}/ai/api/recommendations/explain" "$ai_payload")"
printf '%s\n' "$ai_response"
explanation="$(printf '%s\n' "$ai_response" | jq -r '.explanation // empty')"

if [[ -z "$explanation" ]]; then
  echo "FAIL: AI explanation response did not include an explanation." >&2
  exit 1
fi

echo "PASS: AI explanation endpoint returned an explanation."

echo
echo "Demo flow verified successfully."
