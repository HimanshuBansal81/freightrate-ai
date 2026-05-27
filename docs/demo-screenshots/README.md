# Demo Screenshots

These screenshots show the FreightRate AI local backend MVP running through Docker Compose and being validated through health checks, API demo flow, and CI.

| Screenshot | What it proves |
|---|---|
| [01-github-actions-ci-green.png](01-github-actions-ci-green.png) | GitHub Actions CI validates build, tests, Docker Compose config, and AI service import |
| [02-docker-containers-running.png](02-docker-containers-running.png) | Docker Compose starts Auth, Quote, AI, PostgreSQL, Redis, and Nginx services |
| [03-health-checks.png](03-health-checks.png) | Auth, Quote, and AI services are reachable through local Nginx routing |
| [04-demo-flow-01.png](04-demo-flow-01.png) | Step 1 of the end-to-end API demo flow |
| [05-demo-flow-02.png](05-demo-flow-02.png) | Step 2 of the end-to-end API demo flow |
| [06-demo-flow-03.png](06-demo-flow-03.png) | Step 3 of the end-to-end API demo flow |
| [07-demo-flow-04.png](07-demo-flow-04.png) | Step 4 of the end-to-end API demo flow |
| [08-quote-compare-response.png](08-quote-compare-response.png) | Freight quote comparison returns carrier-wise pricing and recommendation |
| [09-quote-history-response.png](09-quote-history-response.png) | Quote history is persisted and retrievable |
| [10-ai-explanation-response.png](10-ai-explanation-response.png) | AI Recommendation Service returns recommendation explanation or fallback |
| [11-demo-output-files.png](11-demo-output-files.png) | Sanitized demo output files are generated for review |

Do not expose real tokens, API keys, passwords, or `.env` values in screenshots.
