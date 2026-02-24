# Installation Guide

GMCPS is distributed as a Docker image.

- Registry image: `ghcr.io/recepkizilarslan/gmcps`
- Default tag: `latest`
- Package page: <https://github.com/recepkizilarslan/gmcps/pkgs/container/gmcps>

## 1. Prerequisites

- Docker
- Greenbone/GVM stack (local or remote)

## 2. Install and Run (Docker CLI)

```bash
docker pull ghcr.io/recepkizilarslan/gmcps:latest

docker run -d --name gmcps --restart unless-stopped --pull always \
  -p 127.0.0.1:8090:8080 \
  -e GVM_SOCKET_PATH=/run/gvmd/gvmd.sock \
  -e GVM_USERNAME=admin \
  -e GVM_PASSWORD=admin \
  -v /run/gvmd:/run/gvmd \
  ghcr.io/recepkizilarslan/gmcps:latest
```

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

## 6. Update and Version Strategy

Follow latest channel:

```bash
docker pull ghcr.io/recepkizilarslan/gmcps:latest
docker rm -f gmcps
docker run -d --name gmcps --restart unless-stopped --pull always \
  -p 127.0.0.1:8090:8080 \
  -e GVM_SOCKET_PATH=/run/gvmd/gvmd.sock \
  -e GVM_USERNAME=admin \
  -e GVM_PASSWORD=admin \
  -v /run/gvmd:/run/gvmd \
  ghcr.io/recepkizilarslan/gmcps:latest
```

Pin a specific version:

```bash
docker pull ghcr.io/recepkizilarslan/gmcps:1.2.3
docker rm -f gmcps
docker run -d --name gmcps --restart unless-stopped \
  -p 127.0.0.1:8090:8080 \
  -e GVM_SOCKET_PATH=/run/gvmd/gvmd.sock \
  -e GVM_USERNAME=admin \
  -e GVM_PASSWORD=admin \
  -v /run/gvmd:/run/gvmd \
  ghcr.io/recepkizilarslan/gmcps:1.2.3
```

Recommended practice: use `latest` in non-critical environments and pin semver tags in production.

## 7. Troubleshooting

### 7.1 Socket Connection Error

Symptom: tool calls fail with `GMP communication error`.

Check socket:

```bash
ls -l /run/gvmd/gvmd.sock
```

Check logs:

```bash
docker logs --tail 200 gmcps
```

### 7.2 Authentication Error

Symptom: `GMP authentication failed`.

Reset the gvmd admin password in your GVM environment and align `GVM_PASSWORD` in GMCPS container env.

### 7.3 MCP Tools Not Visible

- Confirm URL `http://127.0.0.1:8090/sse`
- Confirm container `gmcps` is running
- Restart MCP client

## 8. Shutdown

```bash
docker rm -f gmcps
```

If you need a clean image refresh:

```bash
docker image rm ghcr.io/recepkizilarslan/gmcps:latest
```
