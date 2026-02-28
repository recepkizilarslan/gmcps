# GMCPS aka OpenVAS MCP Server
[![ci](https://github.com/recepkizilarslan/gmcps/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/recepkizilarslan/gmcps/actions/workflows/ci.yml) [![codeql](https://github.com/recepkizilarslan/gmcps/actions/workflows/codeql.yml/badge.svg?branch=main)](https://github.com/recepkizilarslan/gmcps/actions/workflows/codeql.yml) [![Dependabot Updates](https://github.com/recepkizilarslan/gmcps/actions/workflows/dependabot/dependabot-updates/badge.svg?branch=main)](https://github.com/recepkizilarslan/gmcps/actions/workflows/dependabot/dependabot-updates) [![dependency-review](https://github.com/recepkizilarslan/gmcps/actions/workflows/dependency-review.yml/badge.svg)](https://github.com/recepkizilarslan/gmcps/actions/workflows/dependency-review.yml) [![release](https://github.com/recepkizilarslan/gmcps/actions/workflows/release.yml/badge.svg?branch=main)](https://github.com/recepkizilarslan/gmcps/actions/workflows/release.yml)


Model Context Protocol (MCP) server for Greenbone/OpenVAS (GVM).

- [docs/INSTALLATION.md](docs/INSTALLATION.md)
- [docs/DEVELOPMENT.md](docs/DEVELOPMENT.md)
- [docs/TOOLS.md](docs/TOOLS.md)

## Quick Start

Use with Greenbone Community Containers (default project name: `greenbone-community-edition`):

Replace `<gvmd-username>` and `<gvmd-password>` with your actual gvmd credentials (password is intentionally masked in docs).

```bash
docker pull ghcr.io/recepkizilarslan/gmcps:latest
docker run -d --name gmcps --restart unless-stopped --pull always \
  -p 127.0.0.1:8090:8080 \
  -e GVM_SOCKET_PATH=/run/gvmd/gvmd.sock \
  -e GVM_USERNAME=<gvmd-username> \
  -e GVM_PASSWORD=<gvmd-password> \
  --mount type=volume,src=greenbone-community-edition_gvmd_socket_vol,dst=/run/gvmd \
  ghcr.io/recepkizilarslan/gmcps:latest
```

## Toolsets and Tools

Quick overview. Full tool contracts (inputs/outputs) are in `docs/TOOLS.md`.

### Administration

| Tool | What it does |
|---|---|
| `gvm_get_version` | Returns the connected gvmd/GVM version information. |

### Scans

| Tool | What it does |
|---|---|
| `gvm_create_task` | Creates a new scan task for a target using a scan config (and optional scanner). |
| `gvm_start_task` | Starts a scan task and returns a `reportId` for the run. |
| `gvm_list_tasks` | Lists scan tasks with basic status/progress fields. |
| `gvm_stop_task` | Stops (pauses) a running scan task. |
| `gvm_resume_task` | Resumes a stopped scan task and returns a `reportId`. |
| `gvm_delete_task` | Deletes a scan task (optionally `ultimate` deletion). |
| `gvm_get_task_status` | Gets current task status and progress. |
| `gvm_get_report_summary` | Returns a severity summary for a specific report. |
| `gvm_list_reports` | Lists reports with severity summaries. |
| `gvm_delete_report` | Deletes a report. |
| `gvm_list_results` | Lists individual scan results/findings. |
| `gvm_list_notes` | Lists existing notes (annotations) for findings. |
| `gvm_create_note` | Creates a note to annotate an NVT/result/task/host/port (optionally time-limited). |
| `gvm_modify_note` | Updates an existing note. |
| `gvm_delete_note` | Deletes a note. |
| `gvm_list_overrides` | Lists overrides (e.g., severity adjustments/exceptions) for findings. |
| `gvm_create_override` | Creates an override for an NVT/result/task/host/port (optionally time-limited). |
| `gvm_modify_override` | Updates an existing override. |
| `gvm_delete_override` | Deletes an override. |
| `gvm_get_targets_status` | Returns target status filtered by OS (optionally with tasks and last report summary). |
| `gvm_list_critical_targets` | Lists the most critical targets (sorted by risk or name). |
| `gvm_list_critical_vulnerabilities` | Lists critical vulnerabilities across targets (with scope and severity filters). |
| `gvm_list_critical_packages` | Lists critical packages inferred from scan results (best-effort). |

### Configuration

| Tool | What it does |
|---|---|
| `gvm_list_scan_configs` | Lists available scan configurations. |
| `gvm_list_port_lists` | Lists available port lists. |
| `gvm_list_credentials` | Lists available credentials. |
| `gvm_list_alerts` | Lists configured alerts. |
| `gvm_list_schedules` | Lists configured schedules. |
| `gvm_list_report_configs` | Lists report configurations. |
| `gvm_list_report_formats` | Lists available report formats (e.g., PDF/HTML/XML). |
| `gvm_list_scanners` | Lists configured scanners. |
| `gvm_list_filters` | Lists saved filters. |
| `gvm_list_tags` | Lists existing tags. |
| `gvm_list_users` | Lists users (for assignment/ownership workflows). |
| `gvm_list_targets` | Lists scan targets. |
| `gvm_create_target` | Creates a new scan target (hosts + optional port list). |
| `gvm_delete_target` | Deletes a scan target (optionally `ultimate` deletion). |
| `gvm_set_target_metadata` | Sets target metadata (OS, criticality, tags) used by resilience/status tools. |
| `gvm_get_target_metadata` | Gets target metadata (OS, criticality, tags). |

### Assets

| Tool | What it does |
|---|---|
| `gvm_list_host_assets` | Lists host assets discovered by GVM. |
| `gvm_list_operating_system_assets` | Lists operating system assets with aggregated host/severity stats. |
| `gvm_list_tls_certificates` | Lists TLS certificates observed during scanning. |

### Security Information

| Tool | What it does |
|---|---|
| `gvm_list_nvts` | Lists NVTs (Network Vulnerability Tests) known to GVM. |
| `gvm_list_cves` | Lists CVE entries available in the GVM feed. |
| `gvm_list_cpes` | Lists CPE entries (product identifiers) available in the GVM feed. |
| `gvm_list_cert_bund_advisories` | Lists CERT-Bund advisories available in the GVM feed. |
| `gvm_list_dfn_cert_advisories` | Lists DFN-CERT advisories available in the GVM feed. |

### Resilience

| Tool | What it does |
|---|---|
| `gvm_list_remediation_tickets` | Lists remediation tickets created from findings. |
| `gvm_create_remediation_ticket` | Creates a remediation ticket for a finding and assigns it to a user. |
| `gvm_modify_remediation_ticket` | Updates an existing remediation ticket (status, notes, assignee, comment). |
| `gvm_delete_remediation_ticket` | Deletes a remediation ticket (optionally `ultimate` deletion). |
| `gvm_list_compliance_policies` | Lists available compliance policies. |
| `gvm_list_compliance_audits` | Lists compliance audits (tasks) and their status. |
| `gvm_list_compliance_audit_reports` | Lists compliance audit reports with pass/fail/incomplete summaries. |
| `gvm_is_target_compliant` | Evaluates whether a target is compliant with a given policy. |


## Security Notes

- Input DTO validation on all tools.
- Shared rate limiting for tool execution.
- No raw GMP passthrough.
- GMP communication over Unix socket.
