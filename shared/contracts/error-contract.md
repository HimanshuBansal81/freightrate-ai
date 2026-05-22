# Error Contract

APIs should return a consistent JSON error payload for validation, authorization,
business-rule, and unexpected failures.

```json
{
  "traceId": "00-fd2a7c1c8c1e4d2a9f6a...",
  "statusCode": 422,
  "errorCode": "INVALID_PINCODE",
  "message": "Origin pincode is not serviceable.",
  "details": [
    {
      "field": "originPincode",
      "issue": "No zone mapping found for pincode 999999."
    }
  ],
  "timestamp": "2026-05-14T10:30:00Z"
}
```

## Quote Service Error Codes

- `INVALID_PINCODE`
- `UNSERVICEABLE_ROUTE`
- `NO_ACTIVE_RATE_RULES`
- `INVALID_WEIGHT`
- `INVALID_DIMENSIONS`
- `INVALID_PREFERENCE`
- `QUOTE_NOT_FOUND`
- `AI_SERVICE_UNAVAILABLE`
- `UNAUTHORIZED`
- `FORBIDDEN`
- `INTERNAL_SERVER_ERROR`
