# GMCPS aka OpenVAS MCP Server
[![ci](https://github.com/recepkizilarslan/gmcps/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/recepkizilarslan/gmcps/actions/workflows/ci.yml) [![codeql](https://github.com/recepkizilarslan/gmcps/actions/workflows/codeql.yml/badge.svg?branch=main)](https://github.com/recepkizilarslan/gmcps/actions/workflows/codeql.yml) [![Dependabot Updates](https://github.com/recepkizilarslan/gmcps/actions/workflows/dependabot/dependabot-updates/badge.svg?branch=main)](https://github.com/recepkizilarslan/gmcps/actions/workflows/dependabot/dependabot-updates) [![dependency-review](https://github.com/recepkizilarslan/gmcps/actions/workflows/dependency-review.yml/badge.svg)](https://github.com/recepkizilarslan/gmcps/actions/workflows/dependency-review.yml) [![release](https://github.com/recepkizilarslan/gmcps/actions/workflows/release.yml/badge.svg?branch=main)](https://github.com/recepkizilarslan/gmcps/actions/workflows/release.yml)

A production-grade Model Context Protocol (MCP) server in C# that integrates with Greenbone/OpenVAS (GVM) and enables LLM/agent queries about vulnerability management.

## Architecture

```
src/
  openvas-mcp-server/                   # Single consolidated project
    Program.cs             # Entrypoint, MCP wiring, STDIO/SSE transport
    Tools/                 # 15 MCP tool handlers
    Inputs/                # Input DTOs (one per file)
    Validation/            # Input validation
    Domain/                # Models, interfaces, Result<T>
    Infrastructure/        # GMP clients (Docker/TLS/Unix), XML parsing, SQLite, security
tests/
  openvas-mcp.Tests/             # 45 unit tests (no network)
```

## Prerequisites

- .NET 9 SDK (for local development)
- Docker (for containerized deployment)
- A running GVM/OpenVAS instance (Greenbone Community Edition)

## Documentation

- [Documentation Index](docs/README.md) - Project architecture and technical overview
- [Installation Guide](docs/INSTALLATION.md) - Setup for Claude Desktop, ChatGPT, and transport modes
- [Development Guide](docs/DEVELOPMENT.md) - Local development workflow, build/test, and contribution notes
- [Tool Reference](docs/TOOLS.md) - Full MCP tool contracts, parameters, and examples

## Quick Start: Greenbone Compose Integration

The recommended deployment is adding the MCP server directly to the Greenbone Community Edition `docker-compose.yml`. It runs as a persistent HTTP/SSE service alongside gvmd.

### 1. Build the Docker Image

```bash
cd openvas-mcp
docker build -t openvas-mcp:latest .
```

### 2. Add to Greenbone Compose

Add the `openvas-mcp` service to your `docker-compose.yml`:

```yaml
  openvas-mcp:
    image: openvas-mcp:latest
    restart: on-failure
    ports:
      - 127.0.0.1:8090:8080
    environment:
      - MCP_TRANSPORT=sse
      - GVM_CONNECTION_MODE=unix
      - GVM_SOCKET_PATH=/run/gvmd/gvmd.sock
      - GVM_USERNAME=${GVM_USERNAME:-admin}
      - GVM_PASSWORD=${GVM_PASSWORD:-admin}
    volumes:
      - gvmd_socket_vol:/run/gvmd
      - openvas_mcp_data_vol:/app/data
    depends_on:
      gvmd:
        condition: service_started
```

Add `openvas_mcp_data_vol:` to the `volumes:` section at the bottom.

### 3. Create `.env` File

```bash
echo "GVM_USERNAME=admin" >> .env
echo "GVM_PASSWORD=admin" >> .env
```

### 4. Start

```bash
docker compose up -d openvas-mcp
```

### 5. Configure Claude Desktop (SSE)

```json
{
  "mcpServers": {
    "openvas": {
      "type": "sse",
      "url": "http://localhost:8090/sse"
    }
  }
}
```

## Connection Modes

### Unix Socket Mode (for containers)

When the MCP server runs inside the Greenbone Compose stack, it communicates with gvmd via the shared Unix domain socket. This is the fastest and most reliable mode.

```bash
export GVM_CONNECTION_MODE="unix"
export GVM_SOCKET_PATH="/run/gvmd/gvmd.sock"
```

### Docker Mode (default, for host)

For running the MCP server on the Docker host. Communicates with gvmd via `docker exec` + `socat` through the Unix socket inside the container.

```bash
export GVM_CONNECTION_MODE="docker"
export GVM_CONTAINER_NAME="greenbone-community-edition-gvmd-1"
export GVM_SOCKET_PATH="/run/gvmd/gvmd.sock"
```

### TLS Mode

For GVM instances with GMP exposed over TLS on a TCP port.

```bash
export GVM_CONNECTION_MODE="tls"
export GVM_HOST="localhost"
export GVM_PORT="9390"
```

## Transport Modes

### SSE (HTTP) Transport

For containerized deployment. The server runs as a persistent HTTP service with Server-Sent Events.

```bash
export MCP_TRANSPORT="sse"
```

### STDIO Transport (default)

For local development and direct MCP client integration (Claude Desktop command mode, Claude Code).

```bash
export MCP_TRANSPORT="stdio"  # or omit (default)
```

## Running Locally (STDIO)

```bash
dotnet build
dotnet run --project src/openvas-mcp-server
```

Claude Desktop STDIO configuration:

```json
{
  "mcpServers": {
    "openvas": {
      "command": "dotnet",
      "args": ["run", "--project", "/path/to/src/openvas-mcp-server"],
      "env": {
        "GVM_CONNECTION_MODE": "docker",
        "GVM_CONTAINER_NAME": "greenbone-community-edition-gvmd-1",
        "GVM_SOCKET_PATH": "/run/gvmd/gvmd.sock",
        "GVM_USERNAME": "admin",
        "GVM_PASSWORD": "admin"
      }
    }
  }
}
```

## Environment Variables

| Variable | Required | Default | Description |
|---|---|---|---|
| `MCP_TRANSPORT` | No | `stdio` | Transport mode: `stdio` or `sse` |
| `GVM_CONNECTION_MODE` | No | `docker` | Connection mode: `docker`, `tls`, or `unix` |
| `GVM_CONTAINER_NAME` | No | `greenbone-community-edition-gvmd-1` | Docker container name (docker mode) |
| `GVM_SOCKET_PATH` | No | `/run/gvmd/gvmd.sock` | Unix socket path (docker/unix mode) |
| `GVM_HOST` | No | `localhost` | GVM daemon host (tls mode) |
| `GVM_PORT` | No | `9390` | GVM daemon port (tls mode) |
| `GVM_USERNAME` | No | `admin` | GVM username |
| `GVM_PASSWORD` | No | `admin` | GVM password |

## Available Tools

### Low-Level Tools
| Tool | Description |
|---|---|
| `gvm_get_version` | Get GVM protocol version |
| `gvm_list_scan_configs` | List available scan configurations |
| `gvm_list_targets` | List all scan targets |
| `gvm_create_target` | Create a new scan target |
| `gvm_create_task` | Create a new scan task |
| `gvm_start_task` | Start a scan task |
| `gvm_get_task_status` | Get status of a scan task |
| `gvm_get_report_summary` | Get summary of a scan report |

### Metadata Tools
| Tool | Description |
|---|---|
| `gvm_set_target_metadata` | Set target OS, criticality, tags |
| `gvm_get_target_metadata` | Get target metadata |

### Analytics Tools (NL question answering)
| Tool | Description |
|---|---|
| `gvm_get_targets_status` | "Give me status of all Windows machines" |
| `gvm_list_critical_targets` | "Which targets are critical?" |
| `gvm_list_critical_vulnerabilities` | "List critical vulnerabilities" |
| `gvm_is_target_compliant` | "Is this target compliant?" |
| `gvm_list_critical_packages` | "Which packages are critical?" |

## Sample Tool Calls

### Get status of all Windows targets
```json
{
  "tool": "gvm_get_targets_status",
  "arguments": {
    "os": "Windows",
    "includeTasks": true,
    "includeLastReportSummary": true
  }
}
```

### List critical targets
```json
{
  "tool": "gvm_list_critical_targets",
  "arguments": {
    "minCriticality": "High",
    "sortBy": "Risk"
  }
}
```

### List critical vulnerabilities
```json
{
  "tool": "gvm_list_critical_vulnerabilities",
  "arguments": {
    "scope": { "os": "Any" },
    "minSeverity": 9.0,
    "limit": 20
  }
}
```

### Check target compliance
```json
{
  "tool": "gvm_is_target_compliant",
  "arguments": {
    "targetId": "target-uuid-here",
    "policyId": "no-critical-vulns"
  }
}
```

### Set target metadata
```json
{
  "tool": "gvm_set_target_metadata",
  "arguments": {
    "targetId": "target-uuid-here",
    "os": "Windows",
    "criticality": "Critical",
    "tags": ["production", "web-server"]
  }
}
```

### List critical packages
```json
{
  "tool": "gvm_list_critical_packages",
  "arguments": {
    "os": "Linux",
    "minSeverity": 7.0,
    "limit": 30
  }
}
```

## Compliance Policies

Policies are defined in `data/policies.json`. Each policy has rules that are evaluated against scan findings.

### Policy JSON Schema

```json
[
  {
    "policyId": "no-critical-vulns",
    "name": "No Critical Vulnerabilities",
    "rules": [
      {
        "checkId": "max-sev-check",
        "title": "No findings above severity 7.0",
        "ruleType": "maxSeverity",
        "maxSeverityThreshold": 7.0,
        "requiredNvtOid": null
      }
    ]
  }
]
```

### Rule Types

- **`maxSeverity`**: Fails if any finding exceeds the severity threshold
- **`requiredCheck`**: Fails if the specified NVT OID is not found in scan results

## Security

- **Authentication**: Disabled (URL-only connector setup)
- **Rate Limiting**: Token bucket using a shared `anonymous` bucket
- **Input Validation**: Strict DTO validation with max lengths, enum constraints, range checks
- **No Raw GMP Passthrough**: Only allowlisted operations exposed
- **Unix Socket**: GMP communication uses direct Unix socket (`/run/gvmd/gvmd.sock`) to gvmd
- **Secret Redaction**: Logging never includes passwords or API keys

## Docker Build

```bash
docker build -t openvas-mcp:latest .
```

## Running Tests

```bash
dotnet test
```

45 unit tests covering XML parsing, risk scoring, metadata store, policy evaluation, validation, rate limiting, cancellation, and Result type.

## CI/CD Automation

GitHub Actions workflows are preconfigured:

- `ci`: Runs on pull requests and branch pushes. Executes restore/build/test and validates Docker image build.
- `release`: Runs on semantic version tags (`v*.*.*`). Executes build/test, publishes release artifacts, creates GitHub Release, and pushes multi-arch Docker image to GHCR.
- `dependency-review`: Runs on pull requests and fails if high-severity vulnerable dependencies are introduced.
- `codeql`: Runs on pull requests, pushes to `main`/`master`, and weekly schedule for static security analysis.

Dependabot is configured in `.github/dependabot.yml` for:

- NuGet dependencies (`/src/openvas-mcp-server`, `/tests/openvas-mcp.Tests`)
- Docker base images (`Dockerfile`)
- GitHub Actions versions

## OS Inference Strategy

1. **Metadata store first**: Check SQLite for explicitly set OS/criticality
2. **Target name/comment heuristic**: Infer from keywords (windows, linux, ubuntu, etc.)
3. **Default**: Returns "Unknown" if not confident

## Risk Score Calculation

```
riskScore = high * 10 + medium * 3 + low * 1
```

If no report data exists: `riskScore = 0` with `noData: true`.
