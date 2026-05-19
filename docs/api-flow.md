# API Flow

Initial local routing:

- `/auth/` routes to Auth Service.
- `/quotes/` routes to Quote Service.
- `/carriers/` routes to Quote Service.
- `/zones/` routes to Quote Service.
- `/ai/` routes to AI Recommendation Service.

The first implemented endpoint in each service is `/health`.

Planned quote flow:

1. Client authenticates through Auth Service.
2. Client requests a freight quote from Quote Service.
3. Quote Service applies deterministic pricing rules.
4. Quote Service may request an explanation from AI Recommendation Service.
5. Client receives quotes and optional recommendation explanation.
