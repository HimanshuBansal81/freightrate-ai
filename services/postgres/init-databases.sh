#!/usr/bin/env bash
set -euo pipefail

create_database() {
  local database_name="$1"

  psql -v ON_ERROR_STOP=1 \
    --username "$POSTGRES_USER" \
    --dbname "$POSTGRES_DB" \
    --set database_name="$database_name" <<'SQL'
SELECT 'CREATE DATABASE ' || quote_ident(:'database_name')
WHERE NOT EXISTS (
    SELECT 1 FROM pg_database WHERE datname = :'database_name'
)\gexec
SQL
}

create_database "${AUTH_DB_NAME:-freightrate_auth}"
create_database "${QUOTE_DB_NAME:-freightrate_quote}"
