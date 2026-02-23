# Installation Guide

This document describes how to run `openvas-mcp` with Docker Compose and connect an MCP client over SSE.

## Official Greenbone Community Containers

If you are running the official Greenbone Community container stack, use the official compose file and then attach `openvas-mcp` to the same Docker network/volumes.

Official references:

- Greenbone Community Containers guide: [greenbone.github.io/docs/latest/22.4/container](https://greenbone.github.io/docs/latest/22.4/container/)
- Latest official compose file: [greenbone.github.io/docs/latest/_static/docker-compose.yml](https://greenbone.github.io/docs/latest/_static/docker-compose.yml)
- Docker Engine install docs: [docs.docker.com/engine/install](https://docs.docker.com/engine/install/)
- Docker Compose docs: [docs.docker.com/compose](https://docs.docker.com/compose/)

### 0.1 Start official Greenbone stack

```bash
export DOWNLOAD_DIR="$HOME/greenbone-community-container"
mkdir -p "$DOWNLOAD_DIR"
curl -f -O -L https://greenbone.github.io/docs/latest/_static/docker-compose.yml --output-dir "$DOWNLOAD_DIR"

docker compose -f "$DOWNLOAD_DIR/docker-compose.yml" pull
docker compose -f "$DOWNLOAD_DIR/docker-compose.yml" up -d
```

### 0.2 Attach `openvas-mcp` to the official stack

Build MCP image first (from this repository root):

```bash
docker build -t openvas-mcp:latest .
```

Create `docker-compose.mcp.yml` next to the downloaded Greenbone compose file:

```yaml
services:
  mcp-server:
    image: openvas-mcp:latest
    restart: on-failure
    ports:
      - "127.0.0.1:8090:8080"
    environment:
      - GVM_SOCKET_PATH=/run/gvmd/gvmd.sock
      - GVM_USERNAME=admin
      - GVM_PASSWORD=admin
    volumes:
      - gvmd_socket_vol:/run/gvmd
      - mcp_data_vol:/app/data
    depends_on:
      gvmd:
        condition: service_started

volumes:
  mcp_data_vol:
```

Start with both files:

```bash
docker compose \
  -f "$DOWNLOAD_DIR/docker-compose.yml" \
  -f "$DOWNLOAD_DIR/docker-compose.mcp.yml" \
  up -d
```

## 1. Prerequisites

- Docker
- Docker Compose (included with Docker Desktop)
- A Linux/macOS/Windows host

Note: the repository `docker-compose.yml` starts `gvmd`, `postgres`, `ospd-openvas`, and `mcp-server` together.

## 2. Build and Start

From the repository root:

```bash
docker build -t openvas-mcp:latest .
docker compose up -d --build
```

Check service status:

```bash
docker compose ps
```

Expected: `mcp-server` is running and `127.0.0.1:8090->8080/tcp` is exposed.

## 3. Verify SSE Endpoint

```bash
curl -i -N --max-time 3 http://127.0.0.1:8090/sse
```

Expected SSE event sample:

```text
event: endpoint
data: /message?sessionId=...
```

If this is returned, MCP HTTP transport is active.

## 4. MCP Client Configuration

Use this SSE URL in your MCP client configuration:

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

Server-side variables:

| Variable | Default | Description |
|---|---|---|
| `GVM_SOCKET_PATH` | `/run/gvmd/gvmd.sock` | gvmd Unix socket path |
| `GVM_USERNAME` | `admin` | gvmd username |
| `GVM_PASSWORD` | `admin` | gvmd password |
| `ASPNETCORE_URLS` | `http://+:8080` | MCP server bind address |

## 6. Troubleshooting

### 6.1 Socket Connection Error

Symptom: tool calls return errors similar to `GMP communication error`.

Check:

```bash
docker compose exec gvmd ls -l /run/gvmd/gvmd.sock
```

If the socket does not exist, gvmd may not be fully initialized. Check logs:

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

### 6.3 MCP Tools Not Visible in Client

- Confirm SSE URL is `http://127.0.0.1:8090/sse`.
- Confirm `mcp-server` is running.
- Restart the MCP client.

## 7. Shutdown

```bash
docker compose down
```

To also remove volumes:

```bash
docker compose down -v
```
