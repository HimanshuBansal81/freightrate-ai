# Interview Notes

## Project Pitch

FreightRate AI is a cloud-native backend system that compares freight quotes across carriers. It calculates pricing
deterministically using pincode-zone mapping, volumetric weight, chargeable weight, carrier rate rules, fuel surcharge,
GST, ETA, and user preference. AI is used only to explain the selected recommendation.

## Why AI Is Separate

AI is isolated in its own FastAPI service so pricing remains deterministic, auditable, and testable. The Quote Service
decides the recommendation. The AI service only explains that decision.

## Why Redis

Quote calculations repeatedly read active carriers, pincode-zone mappings, and lane rate rules. Redis reduces read
pressure and latency for those reference lookups. If Redis fails, Quote Service logs a warning and uses PostgreSQL.

## Why JWT Local Validation

Quote Service validates JWTs locally using the Auth Service issuer, audience, and shared secret. This avoids a network call to Auth Service on every protected quote request.

For production, asymmetric JWT signing is preferred: Auth signs tokens with a private key, and other services validate with a public key.

## Why ECS Fargate

ECS Fargate fits because each API service is containerized, stateless at the application layer, and independently
deployable without managing servers. The local Nginx reverse proxy maps naturally to Application Load Balancer
path-based routing in AWS.

Cloud Run is documented as an optional alternative for simple container hosting, but AWS ECS Fargate is the primary deployment path for this repository.

## Balanced Recommendation

Balanced starts with the cheapest option. A faster option qualifies only when:

- It costs no more than 15% above the cheapest option.
- It saves at least one delivery day.

If no faster option qualifies, Balanced returns the cheapest option.

## Redis Failure

Quote Service catches Redis failures, logs warnings, and falls back to PostgreSQL. Redis is never the source of truth.

## AI Failure

Quote Service catches AI failures, timeouts, and invalid responses. It falls back to a deterministic explanation template. AI failure never blocks quote calculation.

## What To Monitor In Production

- Request latency per service
- Quote comparison error rate
- Auth login/register error rate
- PostgreSQL connection and query latency
- Redis hit/miss rate and failures
- AI service latency and fallback rate
- 401/403 spikes
- Nginx or Application Load Balancer 5xx responses
- ECS task restarts, CPU, memory, and service health
