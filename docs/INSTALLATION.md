# Installation Guide

Run GMCPS with Docker Compose and connect an MCP client over SSE.

## 1. Prerequisites

- Docker
- Docker Compose
- Greenbone/GVM stack (local or remote)

Repository `docker-compose.yml` starts `gvmd`, `postgres`, `ospd-openvas`, and `mcp-server` together.

## 2. Build and Start

From repository root:

```bash
docker build -t gmcps:latest .
docker compose up -d --build
```

Check status:

```bash
docker compose ps
```

Expected: `mcp-server` running and `127.0.0.1:8090->8080/tcp` exposed.

## 3. Verify SSE Endpoint

```bash
curl -i -N --max-time 3 http://127.0.0.1:8090/sse
```

Expected sample:

```text
event: endpoint
data: /message?sessionId=...
```

## 4. MCP Client Configuration

Use this SSE URL in the MCP client:

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

## 5. Environment Variables

| Variable | Default | Description |
|---|---|---|
| `GVM_SOCKET_PATH` | `/run/gvmd/gvmd.sock` | gvmd Unix socket path |
| `GVM_USERNAME` | `admin` | gvmd username |
| `GVM_PASSWORD` | `admin` | gvmd password |
| `ASPNETCORE_URLS` | `http://+:8080` | server bind address |

## 6. Troubleshooting

### 6.1 Socket Connection Error

Symptom: tool calls fail with `GMP communication error`.

Check socket:

```bash
docker compose exec gvmd ls -l /run/gvmd/gvmd.sock
```

Check logs:

```bash
docker compose logs --tail 200 gvmd
docker compose logs --tail 200 mcp-server
```

### 6.2 Authentication Error

Symptom: `GMP authentication failed`.

Reset password:

```bash
docker compose exec -u gvmd gvmd gvmd --user=admin --new-password=admin
```

Then align `GVM_PASSWORD` in compose/env.

### 6.3 MCP Tools Not Visible

- Confirm URL `http://127.0.0.1:8090/sse`
- Confirm `mcp-server` is running
- Restart MCP client

## 7. Shutdown

```bash
docker compose down
```

Remove volumes:

```bash
docker compose down -v
```
