# FreightRate AI

Cloud-native multi-carrier freight quote system for comparing carrier pricing, ETA, surcharges, and AI-assisted recommendation explanations.

## Architecture Summary

FreightRate AI is organized as a small monorepo with independent services for authentication, quote calculation, and AI recommendation explanations. Local development is wired through Docker Compose with PostgreSQL, Redis, and Nginx path-based routing.

## Tech Stack

- .NET 8 Web API for Auth Service and Quote Service
- Python FastAPI for AI Recommendation Service
- PostgreSQL for relational data
- Redis for caching
- Nginx for local reverse proxy routing
- Docker Compose for local development

## Services

- `auth-service`: user identity and JWT authentication API shell
- `quote-service`: freight quote, carrier, zone, surcharge, GST, and ETA API shell
- `ai-recommendation-service`: AI-assisted quote explanation API shell
- `postgres`: local relational database
- `redis`: local cache
- `nginx`: local reverse proxy

## Local Development

```bash
docker compose up --build
```

Local routes:

- Auth Service: `http://localhost:5001/health`
- Quote Service: `http://localhost:5002/health`
- AI Recommendation Service: `http://localhost:8000/health`
- Nginx routes: `http://localhost:8080/auth/health`, `http://localhost:8080/quotes/health`, `http://localhost:8080/ai/health`

## Planned Features

- JWT-based authentication and authorization
- Carrier and zone management
- Volumetric weight calculation
- Zone-based freight pricing
- Surcharge and GST calculation
- ETA comparison across carriers
- Redis caching for quote lookups
- AI-assisted explanation layer for quote recommendations

## What AI Does

AI will explain why a quote may be recommended by summarizing structured pricing, ETA, and carrier trade-offs.

## What AI Does Not Do

AI will not calculate freight prices, override deterministic quote rules, decide tax values, or become the source of truth for carrier pricing.

## Current Status

Initial skeleton.
