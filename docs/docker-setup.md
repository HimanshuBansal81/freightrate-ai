# Docker Setup

## Local Environment Setup

Create a local environment file from the tracked example:

```bash
cp .env.example .env
```

Edit `.env` locally if needed. Never commit `.env`.

The local demo works with `AI_PROVIDER=none`. Add real provider keys only to your local `.env` file or a cloud secret manager, never to source control.

Run from the repository root:

```bash
docker compose up --build -d
```

Stop the stack:

```bash
docker compose down
```

Check config:

```bash
docker compose config
```

If any real secret was pushed to GitHub, rotate or revoke it immediately from the provider dashboard. Removing it from the latest commit is not enough because it may remain in Git history.

## Published Ports

- Auth Service: `5001`
- Quote Service: `5002`
- AI Recommendation Service: `8000`
- PostgreSQL: `5432`
- Redis: `6379`
- Nginx: `8080`

## Nginx Routes

- `http://localhost:8080/auth/health`
- `http://localhost:8080/quotes/health`
- `http://localhost:8080/ai/health`

## Common Issues

### Docker daemon not running

Error examples include missing Docker socket or connection refused.

Fix:

```bash
open -a Docker
docker compose ps
```

### Nginx 502 or wrong service response

If service containers are recreated while Nginx stays up, restart Nginx:

```bash
docker compose restart nginx
```

### Wrong docker-compose folder

Run commands from the repository root, where `docker-compose.yml` is located.

### Port conflicts

If ports `5001`, `5002`, `8000`, `8080`, `5432`, or `6379` are already used, stop the conflicting service or change the published port in Compose.

### PostgreSQL volume has old data

The named volume persists data across container restarts. To reset local data:

```bash
docker compose down -v
docker compose up --build -d
```

Only use `-v` when you are comfortable deleting local database state.
