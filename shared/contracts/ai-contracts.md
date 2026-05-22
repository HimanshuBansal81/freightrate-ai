# AI Contracts

Base local route through Nginx: `/ai`.

## Health

`GET /ai/health`

Response:

```json
{
  "status": "healthy",
  "service": "ai-recommendation-service"
}
```

## Explain Recommendation

`POST /ai/api/recommendations/explain`

Request:

```json
{
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
}
```

Response:

```json
{
  "explanation": "Xpressbees is recommended because it best matches your Balanced preference based on the calculated price and delivery time."
}
```

## AI Guardrails

The AI service only explains the backend-selected recommendation.

It must not:

- Calculate prices
- Modify amounts
- Override the recommended carrier
- Invent carriers
- Become the source of truth for freight pricing
