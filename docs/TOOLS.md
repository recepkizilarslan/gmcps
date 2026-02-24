# Toolset Guide

MCP tool contracts grouped by section.

## 1. Common Call Format

All tools expect arguments under `input`.

```json
{
  "jsonrpc": "2.0",
  "id": 10,
  "method": "tools/call",
  "params": {
    "name": "gvm_get_version",
    "arguments": {
      "input": {}
    }
  }
}
```

## 2. Common Behavior

- Rate limit: `60 requests/minute`
- Error response shape: `{"error":"..."}`
- `targetId`, `taskId`, `reportId` fields are validated and length-limited

## 3. Enum Values

| Field | Values |
|---|---|
| `os` (filter) | `Any`, `Windows`, `Linux`, `Unknown` |
| `os` (metadata set) | `Unknown`, `Windows`, `Linux` |
| `criticality` | `Normal`, `High`, `Critical` |
| `sortBy` | `Risk`, `Name` |

## 4. Administration

### `gvm_get_version`
- Input: `{}`
- Response: `{ "version": "22.7" }`

## 5. Scans

### `gvm_create_task`
- Input:
  - `name` (string, required, max 256)
  - `targetId` (string, required, max 128)
  - `scanConfigId` (string, required, max 128)
  - `scannerId` (string, optional, max 128)
- Response: `{ "taskId": "..." }`

### `gvm_start_task`
- Input: `taskId` (string, required)
- Response: `{ "reportId": "..." }`

### `gvm_list_tasks`
- Input:
  - `limit` (`1..1000`, optional, default `50`)
- Response: `{ "tasks": [ { "taskId": "...", "name": "...", "targetId": "...", "scanConfigId": "...", "status": "Running", "progress": 45, "lastReportId": "..." } ] }`

### `gvm_stop_task`
- Input: `taskId` (string, required)
- Response: `{ "ok": true }`

### `gvm_resume_task`
- Input: `taskId` (string, required)
- Response: `{ "reportId": "..." }`

### `gvm_delete_task`
- Input:
  - `taskId` (string, required)
  - `ultimate` (bool, optional, default `true`)
- Response: `{ "ok": true }`

### `gvm_get_task_status`
- Input: `taskId` (string, required)
- Response: `{ "taskId": "...", "name": "...", "status": "Running", "progress": 45, "lastReportId": "..." }`

### `gvm_get_report_summary`
- Input: `reportId` (string, required)
- Response: `{ "reportId": "...", "taskId": "...", "timestamp": "...", "summary": { "high": 0, "medium": 0, "low": 0, "log": 0 } }`

### `gvm_list_reports`
- Input:
  - `limit` (`1..1000`, optional, default `50`)
- Response: `{ "reports": [ { "reportId": "...", "taskId": "...", "timestamp": "...", "summary": { "high": 0, "medium": 0, "low": 0, "log": 0 } } ] }`

### `gvm_delete_report`
- Input: `reportId` (string, required)
- Response: `{ "ok": true }`

### `gvm_list_results`
- Input:
  - `limit` (`1..1000`, optional, default `50`)
- Response: `{ "results": [ { "id": "...", "name": "...", "host": "...", "port": "...", "severity": 0.0, "threat": "...", "nvtOid": "..." } ] }`

### `gvm_list_notes`
- Input:
  - `limit` (`1..1000`, optional, default `50`)
- Response: `{ "notes": [ { "id": "...", "text": "...", "active": true } ] }`

### `gvm_create_note`
- Input:
  - `text` (string, required, max 4096)
  - `nvtOid` (string, required, max 128)
  - `resultId` (string, optional, max 128)
  - `taskId` (string, optional, max 128)
  - `hosts` (string, optional, max 1024)
  - `port` (string, optional, max 256)
  - `severity` (`0..10`, optional)
  - `activeDays` (`-1..3650`, optional)
- Response: `{ "noteId": "..." }`

### `gvm_modify_note`
- Input:
  - `noteId` (string, required)
  - `text` (string, required, max 4096)
  - `nvtOid` (string, optional)
  - `resultId` (string, optional)
  - `taskId` (string, optional)
  - `hosts` (string, optional)
  - `port` (string, optional)
  - `severity` (`0..10`, optional)
  - `activeDays` (`-1..3650`, optional)
- Response: `{ "ok": true }`

### `gvm_delete_note`
- Input:
  - `noteId` (string, required)
  - `ultimate` (bool, optional, default `true`)
- Response: `{ "ok": true }`

### `gvm_list_overrides`
- Input:
  - `limit` (`1..1000`, optional, default `50`)
- Response: `{ "overrides": [ { "id": "...", "text": "...", "newSeverity": 0.0, "active": false } ] }`

### `gvm_create_override`
- Input:
  - `text` (string, required, max 4096)
  - `nvtOid` (string, required, max 128)
  - `newSeverity` (`0..10`, optional)
  - `resultId` (string, optional, max 128)
  - `taskId` (string, optional, max 128)
  - `hosts` (string, optional, max 1024)
  - `port` (string, optional, max 256)
  - `severity` (`0..10`, optional)
  - `activeDays` (`-1..3650`, optional)
- Response: `{ "overrideId": "..." }`

### `gvm_modify_override`
- Input:
  - `overrideId` (string, required)
  - `text` (string, required, max 4096)
  - `nvtOid` (string, optional)
  - `newSeverity` (`0..10`, optional)
  - `resultId` (string, optional)
  - `taskId` (string, optional)
  - `hosts` (string, optional)
  - `port` (string, optional)
  - `severity` (`0..10`, optional)
  - `activeDays` (`-1..3650`, optional)
- Response: `{ "ok": true }`

### `gvm_delete_override`
- Input:
  - `overrideId` (string, required)
  - `ultimate` (bool, optional, default `true`)
- Response: `{ "ok": true }`

### `gvm_get_targets_status`
- Input:
  - `os` (`Any|Windows|Linux|Unknown`, required)
  - `includeTasks` (bool, optional)
  - `includeLastReportSummary` (bool, optional)
- Response: `{ "targets": [ ... ] }`

### `gvm_list_critical_targets`
- Input: `sortBy` (`Risk|Name`, optional, default `Risk`)
- Response: `{ "targets": [ ... ] }`

### `gvm_list_critical_vulnerabilities`
- Input:
  - `scope` (optional)
  - `scope.os` (`Any|Windows|Linux|Unknown`, optional)
  - `scope.targetIds` (string array, optional)
  - `minSeverity` (`0..10`, optional)
  - `limit` (`1..1000`, optional)
- Response: `{ "findings": [ ... ] }`

### `gvm_list_critical_packages`
- Input:
  - `os` (`Any|Windows|Linux|Unknown`, optional)
  - `minSeverity` (`0..10`, optional)
  - `limit` (`1..1000`, optional)
- Response: `{ "packages": [ ... ], "support": "bestEffort" }`

## 6. Configuration

### `gvm_list_scan_configs`
- Input: `{}`
- Response: `{ "configs": [ { "id": "...", "name": "...", "comment": "..." } ] }`

### `gvm_list_port_lists`
- Input:
  - `limit` (`1..1000`, optional, default `50`)
- Response: `{ "portLists": [ { "id": "...", "name": "...", "comment": "..." } ] }`

### `gvm_list_credentials`
- Input:
  - `limit` (`1..1000`, optional, default `50`)
- Response: `{ "credentials": [ { "id": "...", "name": "...", "type": "...", "comment": "..." } ] }`

### `gvm_list_alerts`
- Input:
  - `limit` (`1..1000`, optional, default `50`)
- Response: `{ "alerts": [ { "id": "...", "name": "...", "event": "...", "comment": "..." } ] }`

### `gvm_list_schedules`
- Input:
  - `limit` (`1..1000`, optional, default `50`)
- Response: `{ "schedules": [ { "id": "...", "name": "...", "timezone": "...", "icalendar": "..." } ] }`

### `gvm_list_report_configs`
- Input:
  - `limit` (`1..1000`, optional, default `50`)
- Response: `{ "reportConfigs": [ { "id": "...", "name": "...", "comment": "..." } ] }`

### `gvm_list_report_formats`
- Input:
  - `limit` (`1..1000`, optional, default `50`)
- Response: `{ "reportFormats": [ { "id": "...", "name": "...", "extension": "...", "active": true } ] }`

### `gvm_list_scanners`
- Input:
  - `limit` (`1..1000`, optional, default `50`)
- Response: `{ "scanners": [ { "id": "...", "name": "...", "type": "...", "active": true } ] }`

### `gvm_list_filters`
- Input:
  - `limit` (`1..1000`, optional, default `50`)
- Response: `{ "filters": [ { "id": "...", "name": "...", "term": "...", "type": "..." } ] }`

### `gvm_list_tags`
- Input:
  - `limit` (`1..1000`, optional, default `50`)
- Response: `{ "tags": [ { "id": "...", "name": "...", "value": "...", "comment": "..." } ] }`

### `gvm_list_targets`
- Input: `{}`
- Response: `{ "targets": [ { "id": "...", "name": "...", "tags": [], "hostsCount": 1, "osHint": "Unknown" } ] }`

### `gvm_create_target`
- Input:
  - `name` (string, required, max 256)
  - `hosts` (string, required, max 4096)
  - `comment` (string, optional, max 1024)
- Response: `{ "targetId": "..." }`

### `gvm_set_target_metadata`
- Input:
  - `targetId` (string, required)
  - `os` (`Unknown|Windows|Linux`, required)
  - `criticality` (`Normal|High|Critical`, required)
  - `tags` (string array, optional)
- Response: `{ "ok": true }`

### `gvm_get_target_metadata`
- Input: `targetId` (string, required)
- Response: `{ "targetId": "...", "os": "Linux", "criticality": "Critical", "tags": ["..."] }`

## 7. Assets

### `gvm_list_host_assets`
- Input:
  - `limit` (`1..1000`, optional, default `50`)
- Response: `{ "hosts": [ { "id": "...", "name": "...", "ip": "...", "operatingSystem": "...", "severity": 0.0 } ] }`

### `gvm_list_operating_system_assets`
- Input:
  - `limit` (`1..1000`, optional, default `50`)
- Response: `{ "operatingSystems": [ { "id": "...", "name": "...", "title": "...", "hosts": 0, "allHosts": 0, "averageSeverity": 0.0, "highestSeverity": 0.0 } ] }`

### `gvm_list_tls_certificates`
- Input:
  - `limit` (`1..1000`, optional, default `50`)
- Response: `{ "tlsCertificates": [ { "id": "...", "name": "...", "subjectDn": "...", "issuerDn": "...", "timeStatus": "valid", "sha256Fingerprint": "...", "lastSeen": "..." } ] }`

## 8. Security Information

### `gvm_list_nvts`
- Input:
  - `limit` (`1..1000`, optional, default `50`)
- Response: `{ "nvts": [ { "oid": "...", "name": "...", "family": "...", "severity": 0.0, "summary": "..." } ] }`

### `gvm_list_cves`
- Input:
  - `limit` (`1..1000`, optional, default `50`)
- Response: `{ "cves": [ { "id": "...", "name": "...", "type": "CVE", "score": 9.8, "summary": "..." } ] }`

### `gvm_list_cpes`
- Input:
  - `limit` (`1..1000`, optional, default `50`)
- Response: `{ "cpes": [ { "id": "...", "name": "...", "type": "CPE", "score": null, "summary": "..." } ] }`

### `gvm_list_cert_bund_advisories`
- Input:
  - `limit` (`1..1000`, optional, default `50`)
- Response: `{ "advisories": [ { "id": "...", "name": "...", "type": "CERT_BUND_ADV", "score": null, "summary": "..." } ] }`

### `gvm_list_dfn_cert_advisories`
- Input:
  - `limit` (`1..1000`, optional, default `50`)
- Response: `{ "advisories": [ { "id": "...", "name": "...", "type": "DFN_CERT_ADV", "score": null, "summary": "..." } ] }`

## 9. Resilience

### `gvm_list_remediation_tickets`
- Input:
  - `limit` (`1..1000`, optional, default `50`)
- Response: `{ "tickets": [ { "ticketId": "...", "name": "...", "status": "Open", "severity": 0.0, "host": "...", "location": "...", "resultId": "...", "assignedToUserId": "..." } ] }`

### `gvm_create_remediation_ticket`
- Input:
  - `resultId` (string, required)
  - `assignedToUserId` (string, required)
  - `openNote` (string, required)
  - `comment` (string, optional)
- Response: `{ "ticketId": "..." }`

### `gvm_modify_remediation_ticket`
- Input:
  - `ticketId` (string, required)
  - `status` (string, optional)
  - `openNote` (string, optional)
  - `fixedNote` (string, optional)
  - `closedNote` (string, optional)
  - `assignedToUserId` (string, optional)
  - `comment` (string, optional)
- Response: `{ "ok": true }`

### `gvm_delete_remediation_ticket`
- Input:
  - `ticketId` (string, required)
  - `ultimate` (bool, optional, default `true`)
- Response: `{ "ok": true }`

### `gvm_list_compliance_policies`
- Input: `{}`
- Response: `{ "policies": [ { "policyId": "...", "name": "...", "rules": [ ... ] } ] }`

### `gvm_list_compliance_audits`
- Input:
  - `limit` (`1..1000`, optional, default `50`)
- Response: `{ "audits": [ { "taskId": "...", "name": "...", "status": "...", "progress": 0, "lastReportId": "..." } ] }`

### `gvm_list_compliance_audit_reports`
- Input:
  - `limit` (`1..1000`, optional, default `50`)
- Response: `{ "reports": [ { "reportId": "...", "taskId": "...", "timestamp": "...", "compliance": { "yes": 0, "no": 0, "incomplete": 0 } } ] }`

### `gvm_is_target_compliant`
- Input:
  - `targetId` (string, required)
  - `policyId` (string, required)
- Response:
  - Pass/fail/no-data compliance result with evidence array

Default policy IDs (from `data/policies.json`):
- `no-critical-vulns`
- `strict-compliance`
- `ssh-hardening`

## 10. Error Examples

Validation error:

```json
{ "error": "Validation failed: ..." }
```

ID validation error:

```json
{ "error": "targetId contains invalid characters" }
```

Rate-limit error:

```json
{ "error": "Rate limit exceeded. Please wait and try again." }
```
