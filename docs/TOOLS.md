# Toolset Guide

This document defines MCP tools and their input/output contracts.

## 1. Common Call Format

All tools in this project use the `input` wrapper in `tools/call.arguments`.

Example payload:

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

- Rate limit: `60 requests/minute` (shared anonymous bucket)
- Error response shape: `{"error":"..."}`
- `targetId`, `taskId`, `reportId` are validated and limited to `128` characters

## 3. Enum Values

| Field | Values |
|---|---|
| `os` (filter) | `Any`, `Windows`, `Linux`, `Unknown` |
| `os` (metadata set) | `Unknown`, `Windows`, `Linux` |
| `criticality` | `Normal`, `High`, `Critical` |
| `sortBy` | `Risk`, `Name` |

## 4. Low-Level Tools (8)

### `gvm_get_version`
- Input: `{}`
- Response: `{ "version": "22.7" }`

### `gvm_list_scan_configs`
- Input: `{}`
- Response: `{ "configs": [ { "id": "...", "name": "...", "comment": "..." } ] }`

### `gvm_list_targets`
- Input: `{}`
- Response: `{ "targets": [ { "id": "...", "name": "...", "tags": [], "hostsCount": 1, "osHint": "Unknown" } ] }`

### `gvm_create_target`
- Input:
  - `name` (string, required, max 256)
  - `hosts` (string, required, max 4096)
  - `comment` (string, optional, max 1024)
- Response: `{ "targetId": "..." }`

Example:

```json
{
  "input": {
    "name": "Prod Web",
    "hosts": "192.168.1.10,192.168.1.11",
    "comment": "Production web servers"
  }
}
```

### `gvm_create_task`
- Input:
  - `name` (string, required, max 256)
  - `targetId` (string, required, max 128)
  - `scanConfigId` (string, required, max 128)
  - `scannerId` (string, optional, max 128, defaults to OpenVAS default scanner)
- Response: `{ "taskId": "..." }`

### `gvm_start_task`
- Input:
  - `taskId` (string, required)
- Response: `{ "reportId": "..." }`

### `gvm_get_task_status`
- Input:
  - `taskId` (string, required)
- Response: `{ "taskId": "...", "name": "...", "status": "Running", "progress": 45, "lastReportId": "..." }`

### `gvm_get_report_summary`
- Input:
  - `reportId` (string, required)
- Response: `{ "reportId": "...", "taskId": "...", "timestamp": "...", "summary": { "high": 0, "medium": 0, "low": 0, "log": 0 } }`

## 5. Metadata Tools (2)

### `gvm_set_target_metadata`
- Input:
  - `targetId` (string, required, max 128)
  - `os` (`Unknown|Windows|Linux`, required)
  - `criticality` (`Normal|High|Critical`, required)
  - `tags` (string array, optional, max 50 items)
- Response: `{ "ok": true }`

Example:

```json
{
  "input": {
    "targetId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "os": "Linux",
    "criticality": "Critical",
    "tags": ["production", "dmz"]
  }
}
```

### `gvm_get_target_metadata`
- Input:
  - `targetId` (string, required, max 128)
- Response: `{ "targetId": "...", "os": "Linux", "criticality": "Critical", "tags": ["..."] }`

## 6. Analytics Tools (5)

### `gvm_get_targets_status`
- Input:
  - `os` (`Any|Windows|Linux|Unknown`, required)
  - `includeTasks` (bool, optional, default false)
  - `includeLastReportSummary` (bool, optional, default false)
- Response: `{ "targets": [ ... ] }`

### `gvm_list_critical_targets`
- Input:
  - `sortBy` (`Risk|Name`, optional, default `Risk`)
- Response: `{ "targets": [ { "targetId": "...", "criticality": "...", "riskScore": 0, "noData": true, "lastSummary": null } ] }`

### `gvm_list_critical_vulnerabilities`
- Input:
  - `scope` (optional object)
  - `scope.os` (`Any|Windows|Linux|Unknown`, optional, default `Any`)
  - `scope.targetIds` (string array, optional, max 100)
  - `minSeverity` (number, optional, `0..10`, default `9.0`)
  - `limit` (int, optional, `1..1000`, default `50`)
- Response: `{ "findings": [ { "name": "...", "severity": 9.8, "qod": 80, "cves": [], "affectedHosts": 2, "topHosts": [], "nvtOid": "..." } ] }`

### `gvm_is_target_compliant`
- Input:
  - `targetId` (string, required)
  - `policyId` (string, required)
- Response:
  - If report data exists: `{ "targetId": "...", "policyId": "...", "compliant": true, "status": "Pass", "evidence": [...] }`
  - If report data does not exist: `{ "targetId": "...", "policyId": "...", "compliant": false, "status": "NoData", "evidence": [] }`

Default policy IDs (from `data/policies.json`):
- `no-critical-vulns`
- `strict-compliance`
- `ssh-hardening`

### `gvm_list_critical_packages`
- Input:
  - `os` (`Any|Windows|Linux|Unknown`, optional, default `Any`)
  - `minSeverity` (number, optional, `0..10`, default `7.0`)
  - `limit` (int, optional, `1..1000`, default `50`)
- Response:
  - If packages are extracted: `{ "packages": [ { "packageName": "...", "severity": 9.1, "affectedHosts": 2, "evidence": [...] } ], "support": "bestEffort" }`
  - If no packages are extracted: `{ "packages": [], "support": "bestEffort", "explanation": "..." }`

## 7. Error Examples

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
