# Docker Setup

Run the local stack from the repository root:

```bash
docker compose up --build
```

Published ports:

- Auth Service: `5001`
- Quote Service: `5002`
- AI Recommendation Service: `8000`
- PostgreSQL: `5432`
- Redis: `6379`
- Nginx: `8080`

Nginx routes requests to the internal service network.
