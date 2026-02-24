# GMCPS aka OpenVAS MCP Server
[![ci](https://github.com/recepkizilarslan/gmcps/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/recepkizilarslan/gmcps/actions/workflows/ci.yml) [![codeql](https://github.com/recepkizilarslan/gmcps/actions/workflows/codeql.yml/badge.svg?branch=main)](https://github.com/recepkizilarslan/gmcps/actions/workflows/codeql.yml) [![Dependabot Updates](https://github.com/recepkizilarslan/gmcps/actions/workflows/dependabot/dependabot-updates/badge.svg?branch=main)](https://github.com/recepkizilarslan/gmcps/actions/workflows/dependabot/dependabot-updates) [![dependency-review](https://github.com/recepkizilarslan/gmcps/actions/workflows/dependency-review.yml/badge.svg)](https://github.com/recepkizilarslan/gmcps/actions/workflows/dependency-review.yml) [![release](https://github.com/recepkizilarslan/gmcps/actions/workflows/release.yml/badge.svg?branch=main)](https://github.com/recepkizilarslan/gmcps/actions/workflows/release.yml)


Production-grade Model Context Protocol (MCP) server in C# for Greenbone/OpenVAS (GVM).

- [docs/README.md](docs/README.md)
- [docs/INSTALLATION.md](docs/INSTALLATION.md)
- [docs/DEVELOPMENT.md](docs/DEVELOPMENT.md)
- [docs/TOOLS.md](docs/TOOLS.md)

## Prerequisites

- .NET 9 SDK (local development)
- Docker + Docker Compose (container deployment)
- Running Greenbone/GVM stack

## Quick Start (Docker Compose)

```bash
docker compose up -d --build
```

SSE endpoint check:

```bash
curl -i -N --max-time 3 http://127.0.0.1:8090/sse
```

Expected response includes an `event: endpoint` with `sessionId`.

## Local Run

```bash
dotnet restore
dotnet build
dotnet run --project src/Gmcps
```

## MCP Client Config (SSE)

```json
{
  "mcpServers": {
    "openvas": {
      "type": "sse",
      "url": "http://127.0.0.1:8090/sse"
    }
  }
}
```

## Environment Variables

| Variable | Default | Description |
|---|---|---|
| `GVM_SOCKET_PATH` | `/run/gvmd/gvmd.sock` | gvmd Unix socket path |
| `GVM_USERNAME` | `admin` | gvmd username |
| `GVM_PASSWORD` | `admin` | gvmd password |
| `ASPNETCORE_URLS` | `http://+:8080` | HTTP bind address |

## Toolsets and Tools

### Administration
- `gvm_get_version`

### Scans
- `gvm_create_task`
- `gvm_start_task`
- `gvm_list_tasks`
- `gvm_stop_task`
- `gvm_resume_task`
- `gvm_delete_task`
- `gvm_get_task_status`
- `gvm_get_report_summary`
- `gvm_list_reports`
- `gvm_delete_report`
- `gvm_list_results`
- `gvm_list_notes`
- `gvm_create_note`
- `gvm_modify_note`
- `gvm_delete_note`
- `gvm_list_overrides`
- `gvm_create_override`
- `gvm_modify_override`
- `gvm_delete_override`
- `gvm_get_targets_status`
- `gvm_list_critical_targets`
- `gvm_list_critical_vulnerabilities`
- `gvm_list_critical_packages`

### Configuration
- `gvm_list_scan_configs`
- `gvm_list_port_lists`
- `gvm_list_credentials`
- `gvm_list_alerts`
- `gvm_list_schedules`
- `gvm_list_report_configs`
- `gvm_list_report_formats`
- `gvm_list_scanners`
- `gvm_list_filters`
- `gvm_list_tags`
- `gvm_list_targets`
- `gvm_create_target`
- `gvm_set_target_metadata`
- `gvm_get_target_metadata`

### Assets
- `gvm_list_host_assets`
- `gvm_list_operating_system_assets`
- `gvm_list_tls_certificates`

### Security Information
- `gvm_list_nvts`
- `gvm_list_cves`
- `gvm_list_cpes`
- `gvm_list_cert_bund_advisories`
- `gvm_list_dfn_cert_advisories`

### Resilience
- `gvm_list_remediation_tickets`
- `gvm_create_remediation_ticket`
- `gvm_modify_remediation_ticket`
- `gvm_delete_remediation_ticket`
- `gvm_list_compliance_policies`
- `gvm_list_compliance_audits`
- `gvm_list_compliance_audit_reports`
- `gvm_is_target_compliant`

## Example Tool Calls

```json
{
  "tool": "gvm_list_host_assets",
  "arguments": {
    "limit": 25
  }
}
```

```json
{
  "tool": "gvm_list_cves",
  "arguments": {
    "limit": 50
  }
}
```

```json
{
  "tool": "gvm_is_target_compliant",
  "arguments": {
    "targetId": "target-uuid-here",
    "policyId": "no-critical-vulns"
  }
}
```

## Security Notes

- Input DTO validation on all tools.
- Shared rate limiting for tool execution.
- No raw GMP passthrough.
- GMP communication over Unix socket.

## Tests

```bash
dotnet test
```
