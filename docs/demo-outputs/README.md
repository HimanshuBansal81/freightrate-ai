# Demo Outputs

This folder stores sanitized API outputs generated from the local running Docker Compose system.

These files are optional demo proof for recruiters and interviewers. They should reflect real runtime responses from the local services, not hand-written or mocked responses.

Before committing generated output files:

- Redact JWT tokens and any other secrets.
- Do not include `Authorization` headers.
- Do not include real API keys or cloud secrets.
- Review the generated JSON files for sensitive values.

Generate fresh outputs from the repository root:

```bash
./scripts/generate-demo-outputs.sh
```

