# Demo Outputs

This folder stores sanitized API outputs captured from the local Docker Compose system.

These files provide optional demo proof for project review. They should reflect real runtime responses from the local services, not hand-written or mocked responses.

Before committing refreshed output files:

- Redact JWT tokens and any other secrets.
- Do not include `Authorization` headers.
- Do not include real API keys or cloud secrets.
- Review the generated JSON files for sensitive values.

Generate fresh outputs from the repository root:

```bash
./scripts/generate-demo-outputs.sh
```
