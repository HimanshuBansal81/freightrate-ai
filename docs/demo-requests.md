# Demo Requests

Base URL for local Docker Compose:

```bash
BASE_URL=http://localhost:8080
```

No real AI provider key is required when `AI_PROVIDER=none`.

## Health Checks

```bash
curl -sS "$BASE_URL/auth/health"
curl -sS "$BASE_URL/quotes/health"
curl -sS "$BASE_URL/ai/health"
```

## Register

```bash
curl -sS -X POST "$BASE_URL/auth/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "fullName": "Demo Shipper",
    "email": "demo.shipper@example.com",
    "password": "Passw0rd!",
    "role": "Shipper"
  }'
```

## Login

```bash
TOKEN=$(curl -sS -X POST "$BASE_URL/auth/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "demo.shipper@example.com",
    "password": "Passw0rd!"
  }' | python3 -c 'import json,sys; print(json.load(sys.stdin)["token"])')

echo "$TOKEN"
```

## Compare Quote

```bash
curl -sS -X POST "$BASE_URL/quotes/api/quotes/compare" \
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

Expected deterministic calculation for the current seeded data:

- `volumetricWeightKg`: `6`
- `chargeableWeightKg`: `8`
- Xpressbees total: around `166.14`
- Delhivery total: around `192.95`
- BlueDart total: around `284.97`
- `Balanced` recommends Xpressbees because Delhivery exceeds the 15% threshold above the cheapest option.

## Quote History

```bash
curl -sS "$BASE_URL/quotes/api/quotes/history" \
  -H "Authorization: Bearer $TOKEN"
```

## Quote By ID

Set `QUOTE_ID` to a value returned by compare or history.

```bash
QUOTE_ID=1

curl -sS "$BASE_URL/quotes/api/quotes/$QUOTE_ID" \
  -H "Authorization: Bearer $TOKEN"
```

## Carriers

```bash
curl -sS "$BASE_URL/quotes/api/carriers"
```

## Zones

```bash
curl -sS "$BASE_URL/quotes/api/zones"
```

## AI Explanation Endpoint

```bash
curl -sS -X POST "$BASE_URL/ai/api/recommendations/explain" \
  -H "Content-Type: application/json" \
  -d '{
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
```

