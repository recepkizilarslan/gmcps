# Development Guide

This document summarizes local development workflow and project conventions.

## 1. Stack

- Language/runtime: C# / .NET 9
- Transport: MCP over HTTP/SSE
- GVM access: Unix socket (`/run/gvmd/gvmd.sock`)
- Tests: xUnit (`tests/Gmcps.Tests`)

## 2. Current Structure

| Path | Purpose |
|---|---|
| `src/Gmcps/Program.cs` | startup, DI, MCP transport and toolset registration |
| `src/Gmcps/Core/Abstractions/` | `ITool`, `IClient`, `IToolset` abstractions |
| `src/Gmcps/Toolsets/` | MCP section surfaces (delegation only) |
| `src/Gmcps/Tools/` | tool implementations + section/domain inputs/outputs |
| `src/Gmcps/Domain/` | `Result`, domain models, validation attributes |
| `src/Gmcps/Infrastructure/Clients/Gvm/` | socket GVM client + XML parsing |
| `src/Gmcps/Infrastructure/Security/` | validation and rate limiter |
| `src/Gmcps/Infrastructure/Stores/` | metadata/policy stores |
| `src/Gmcps/ServiceCollectionExtensions.cs` | composition root extensions |
| `tests/Gmcps.Tests/` | unit tests |
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
dotnet publish src/Gmcps/Gmcps.csproj -c Release -o publish
```

## 4. Local Run

### 4.1 Container Smoke Run (GHCR latest)

Replace `<gvmd-username>` and `<gvmd-password>` with your actual gvmd credentials.

```bash
docker pull ghcr.io/recepkizilarslan/gmcps:latest
docker run --rm \
  -p 127.0.0.1:8090:8080 \
  -e GVM_SOCKET_PATH=/run/gvmd/gvmd.sock \
  -e GVM_USERNAME=<gvmd-username> \
  -e GVM_PASSWORD=<gvmd-password> \
  -v /run/gvmd:/run/gvmd \
  ghcr.io/recepkizilarslan/gmcps:latest
```

### 4.2 Direct dotnet run

```bash
GVM_SOCKET_PATH=/run/gvmd/gvmd.sock \
GVM_USERNAME=<gvmd-username> \
GVM_PASSWORD=<gvmd-password> \
dotnet run --project src/Gmcps --urls http://127.0.0.1:5199
```

SSE endpoint smoke check:

```bash
curl -i -N --max-time 3 http://127.0.0.1:5199/sse
```

## 5. MCP Smoke Test (`tools/list`)

Handshake flow:

1. `GET /sse` and read `sessionId`
2. `POST /message?sessionId=...` with `initialize`
3. send `notifications/initialized`
4. call `tools/list`

Tool arguments are expected under `{"input": {...}}`.

## 6. Configuration Sources

- Defaults: `src/Gmcps/appsettings.json`
- Environment overrides: `GVM_SOCKET_PATH`, `GVM_USERNAME`, `GVM_PASSWORD`

## 7. Coding Rules

When adding a new tool:

1. Add input/output files under `src/Gmcps/Tools/<Section>/<Domain>/<ToolName>/`
2. Add tool implementation slice under `src/Gmcps/Tools/<Section>/<Domain>/<ToolName>/`
3. Keep all GMP XML request/response + parse operations in `Infrastructure/Clients/Gvm`
4. Register `IClient` and `ITool` in `ServiceCollectionExtensions`
5. Expose tool in `src/Gmcps/Toolsets/<Section>Toolset.cs`

Additional rules:

- Toolset classes must not contain business logic.
- Tool classes own flow orchestration and validation.
- Tool classes must not depend on concrete client implementations; depend on `IClient`.
- XML-escape dynamic GMP values.
- Keep error response contract as `{"error":"..."}`.

## 8. Test Strategy

- Unit tests run without network dependency.
- Coverage includes parser, validation, stores, policy evaluation, rate limiting, cancellation.
- Add parser + tool flow tests for each new tool contract.

## 9. CI/CD

Workflow files:

- `.github/workflows/ci.yml`
- `.github/workflows/release.yml`
- `.github/workflows/dependency-review.yml`
- `.github/workflows/codeql.yml`

Summary:

- `ci`: restore/build/test + Docker build validation
- `release`: GitHub Release + GHCR multi-arch Docker image publish on `v*.*.*`
- `dependency-review`: dependency security gate on PR
- `codeql`: static security analysis
