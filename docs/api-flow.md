# API Flow

## Auth Flow

1. Client registers through `POST /auth/api/auth/register`.
2. Auth Service validates the request, hashes the password, stores the user, assigns a role, and returns a JWT.
3. Client logs in through `POST /auth/api/auth/login`.
4. Client sends `Authorization: Bearer <token>` to protected Quote Service endpoints.
5. Quote Service validates JWT locally using the same issuer, audience, and shared secret.

## Quote Compare Flow

1. Client calls `POST /quotes/api/quotes/compare`.
2. Quote Service validates JWT role: `Shipper` or `Admin`.
3. Quote Service validates pincode, weight, dimensions, and preference.
4. Quote Service resolves origin and destination zones.
5. Quote Service fetches active carrier rate rules for the lane.
6. Quote Service calculates:
   - Volumetric weight
   - Chargeable weight
   - Base freight
   - Fuel surcharge
   - GST
   - Total amount
7. Quote Service selects the backend recommendation using Cheapest, Fastest, or Balanced rules.
8. Quote Service asks AI Recommendation Service for an explanation.
9. Quote Service saves `QuoteRequest` and `QuoteOption` rows.
10. Quote Service returns the quote response.

## Redis Cache Flow

Quote Service uses Redis as a read-through cache for:

- `zone:pincode:{pincode}`
- `rate-rules:{originZone}:{destinationZone}`
- `carriers:active`

On cache hit, Quote Service uses cached reference data. On cache miss, Quote Service reads PostgreSQL and writes Redis with a TTL.

PostgreSQL remains the source of truth.

## AI Explanation Flow

1. Quote Service calculates all prices itself.
2. Quote Service selects the recommended carrier itself.
3. Quote Service sends only structured recommendation context to AI Service.
4. AI Service returns a concise explanation.
5. Quote Service stores and returns that explanation.

AI cannot alter carrier selection or money values.

## Failure Fallback Flow

- Redis down: log warning, use PostgreSQL.
- AI service down: log warning, use deterministic explanation template.
- Unknown pincode: return `422 INVALID_PINCODE`.
- No rate rules: return `422 NO_ACTIVE_RATE_RULES`.
- Missing or invalid JWT: return `401 UNAUTHORIZED`.
- Wrong role: return `403 FORBIDDEN`.
