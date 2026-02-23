# Development Guide

This document summarizes the development workflow and technical conventions for the project.

## 1. Stack and Architecture

- Language/Runtime: C# / .NET 9
- Project type: single service (`src/openvas-mcp-server`)
- Tests: xUnit (`tests/openvas-mcp-tests`)
- MCP transport: HTTP/SSE
- GVM connection: Unix socket (`/run/gvmd/gvmd.sock`)

## 2. Directory Layout

| Path | Purpose |
|---|---|
| `src/openvas-mcp-server/Program.cs` | service entrypoint, DI, MCP registration |
| `src/openvas-mcp-server/Tools/` | 15 MCP tool implementations |
| `src/openvas-mcp-server/Inputs/` | tool input DTOs |
| `src/openvas-mcp-server/Infrastructure/Gmp/` | Unix socket GMP client and XML parser |
| `src/openvas-mcp-server/Infrastructure/Stores/` | SQLite metadata and JSON policy stores |
| `tests/openvas-mcp-tests/` | unit tests |
| `data/policies.json` | compliance policy definitions |

## 3. Standard Commands

From repository root:

```bash
dotnet restore
dotnet build
dotnet test
```

Release publish:

```bash
dotnet publish src/openvas-mcp-server/openvas-mcp.csproj -c Release -o publish
```

## 4. Local Run

### 4.1 Docker Compose (recommended)

```bash
docker compose up -d --build
```

### 4.2 Direct dotnet run

If the host has access to the gvmd socket:

```bash
GVM_SOCKET_PATH=/run/gvmd/gvmd.sock \
GVM_USERNAME=admin \
GVM_PASSWORD=admin \
dotnet run --project src/openvas-mcp-server --urls http://127.0.0.1:5199
```

SSE endpoint test:

```bash
curl -i -N --max-time 3 http://127.0.0.1:5199/sse
```

## 5. MCP Smoke Test (tools/list)

Handshake flow:

1. `GET /sse` and read `sessionId`
2. `POST /message?sessionId=...` with `initialize`
3. send `notifications/initialized`
4. call `tools/list`

Note: this project expects tool arguments under `{"input": {...}}`.

## 6. Configuration Sources

- Static defaults: `src/openvas-mcp-server/appsettings.json`
- Environment overrides: `GVM_SOCKET_PATH`, `GVM_USERNAME`, `GVM_PASSWORD`

Default rate limit in `Server` section: `60 req/min`.

## 7. Coding Rules

- When adding a tool:
  - add input type under `src/openvas-mcp-server/Inputs/`
  - add method in the relevant tool class (`LowLevelTools`, `MetadataTools`, `AnalyticsTools`)
  - apply validation with `InputValidator`
- XML-escape all dynamic values in GMP commands
- Keep error response format as `{"error":"..."}`

## 8. Test Strategy

- Unit tests run without network dependency
- Coverage includes parser, validation, policy evaluation, rate limiter, and store layers
- Add tests for any new tool behavior or contract change

## 9. CI/CD

Workflow files:

- `.github/workflows/ci.yml`
- `.github/workflows/release.yml`
- `.github/workflows/dependency-review.yml`
- `.github/workflows/codeql.yml`

Flow summary:

- `ci`: restore/build/test + Docker build validation on PR/push
- `release`: artifact + GitHub Release + GHCR image push on `v*.*.*` tag
- `dependency-review`: dependency security gate on PR
- `codeql`: static security analysis

Create a release:

```bash
git tag v1.0.0
git push origin v1.0.0
```
