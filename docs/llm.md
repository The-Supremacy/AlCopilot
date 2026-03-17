# AlCopilot — Local LLM & Vector Database Setup

## Overview

This document describes the local development environment for running an LLM and vector database alongside the AlCopilot .NET Aspire application.

The setup runs **Mistral 7B via Ollama** on the Windows host (with GPU acceleration) and **Qdrant** inside the Hyper-V Ubuntu VM alongside the AlCopilot application. The LLM is exposed as an HTTP API so the VM can reach it over the Hyper-V Default Switch network.

---

## Architecture

```
┌──────────────────────────────────────────────────────────┐
│  Windows Host  (RTX 4080 SUPER — 16 GB VRAM)             │
│                                                          │
│  ┌────────────────────────────────────────────────────┐  │
│  │  Ollama  (native Windows process)                  │  │
│  │  - Model: mistral:7b  (~4.5 GB VRAM, Q4_K_M)      │  │
│  │  - Listens on 0.0.0.0:11434                        │  │
│  │  - Start/stop instantly — just a process           │  │
│  └────────────────────┬───────────────────────────────┘  │
│                       │ HTTP API  (Hyper-V Default Switch)│
│  ┌────────────────────▼───────────────────────────────┐  │
│  │  Hyper-V Ubuntu VM  (100 GB disk / 16 GB RAM)      │  │
│  │                                                    │  │
│  │  ┌──────────────────┐   ┌────────────────────────┐ │  │
│  │  │  AlCopilot app   │   │  Qdrant (Docker)       │ │  │
│  │  │  (.NET Aspire)   │◄──│  CPU-only, port 6333   │ │  │
│  │  └──────────────────┘   └────────────────────────┘ │  │
│  └────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────┘
```

GPU passthrough from Hyper-V to a Linux VM is not supported for consumer GPUs, so the LLM runs on the host and is consumed as a plain HTTP endpoint by the VM. Qdrant is CPU-only and lightweight enough to run comfortably inside the VM.

---

## Hardware Requirements

| Component | Minimum | Recommended |
|-----------|---------|-------------|
| GPU VRAM (host) | 6 GB | 8 GB+ (16 GB for headroom) |
| System RAM (VM) | 8 GB | 16 GB |
| VM disk | 40 GB | 100 GB |
| Host OS | Windows 10/11 with Hyper-V | — |

The RTX 4080 SUPER (16 GB) leaves ~11 GB of VRAM free when running `mistral:7b` at Q4_K_M quantization, so normal desktop use and other GPU workloads are not affected.

---

## Ollama Setup (Windows Host)

### Install Ollama

Download and install from [https://ollama.com](https://ollama.com). The installer adds `ollama` to your `PATH`.

### Pull the model

```powershell
ollama pull mistral:7b
```

This downloads the `mistral:7b` model (~4.1 GB). It is stored in `%USERPROFILE%\.ollama\models` by default.

### Allow network access from the VM

By default Ollama listens only on `localhost`. Set `OLLAMA_HOST` so it accepts connections from the Hyper-V VM:

**Option A — per-session (PowerShell)**

```powershell
$env:OLLAMA_HOST = "0.0.0.0"
ollama serve
```

**Option B — persistent user environment variable**

```powershell
[System.Environment]::SetEnvironmentVariable("OLLAMA_HOST", "0.0.0.0", "User")
```

After setting the variable, restart any open terminals and re-run `ollama serve`.

### Verify Ollama is running

From the Windows host:

```powershell
curl http://localhost:11434/api/tags
```

From the Hyper-V VM (replace `<host-ip>` with the actual host IP — see [Connectivity](#connectivity)):

```bash
curl http://<host-ip>:11434/api/tags
```

A JSON response listing the available models confirms it is reachable.

---

## Starting and Stopping the LLM

Full on/off control is a first-class requirement. Ollama is a plain process — there is no persistent service by default.

### Start

```powershell
# In a new terminal on the Windows host:
ollama serve
```

Or run a model directly (also starts the server):

```powershell
ollama run mistral:7b
```

### Stop

Close the terminal window, or press **Ctrl+C** in the Ollama terminal. The process exits immediately and all VRAM is released.

From another terminal:

```powershell
taskkill /IM ollama.exe /F
```

### Check VRAM usage

```powershell
nvidia-smi
```

After stopping Ollama, the VRAM usage should drop back to idle levels within a few seconds.

> **Important**: Ollama is **not** registered as a Windows service by default. It starts only when you explicitly run it and stops the moment the process ends. If you want it to start automatically you would need to configure it as a service — avoid doing that if instant on/off control is the goal.

---

## Qdrant Setup (Hyper-V VM)

### Run via Docker

```bash
docker run -d \
  --name qdrant \
  -p 6333:6333 \
  -p 6334:6334 \
  -v qdrant_storage:/qdrant/storage \
  qdrant/qdrant
```

| Port | Protocol | Purpose |
|------|----------|---------|
| 6333 | HTTP/REST | REST API and Qdrant Web UI |
| 6334 | gRPC | gRPC API |

### Verify Qdrant is running

```bash
curl http://localhost:6333/healthz
```

The Qdrant Web UI is available at [http://localhost:6333/dashboard](http://localhost:6333/dashboard).

### Persistent storage

The `-v qdrant_storage:/qdrant/storage` flag maps a named Docker volume so the vector data survives container restarts. To wipe and start fresh:

```bash
docker rm -f qdrant && docker volume rm qdrant_storage
```

---

## Aspire Integration

Both Ollama and Qdrant have community Aspire hosting packages.

### NuGet packages

Add these to the `AlCopilot.AppHost` project:

```xml
<!-- Ollama hosting integration (check NuGet for the current version) -->
<PackageReference Include="CommunityToolkit.Aspire.Hosting.Ollama" Version="9.1.0" />

<!-- Qdrant hosting integration (check NuGet for the current version) -->
<PackageReference Include="Aspire.Hosting.Qdrant" Version="9.1.0" />
```

### AppHost wiring

Because Ollama runs on the Windows host (not inside Aspire), it is referenced as an external resource rather than a managed container. Qdrant is a normal Aspire-managed container that runs inside the VM.

```csharp
// AppHost/Program.cs
var builder = DistributedApplication.CreateBuilder(args);

// Ollama: external process on the Windows host.
// Replace the URL with the actual host IP (see Connectivity section).
var ollama = builder.AddConnectionString(
    "ollama",
    builder.Configuration["Ollama:Endpoint"] ?? "http://<HOST_IP>:11434");

// Qdrant: Aspire-managed container inside the VM.
var qdrant = builder.AddQdrant("qdrant");

var api = builder.AddProject<Projects.AlCopilot_Api>("api")
    .WithReference(ollama)
    .WithReference(qdrant);

builder.Build().Run();
```

Store the Ollama endpoint in `appsettings.Development.json` so each developer can set their own host IP:

```json
{
  "Ollama": {
    "Endpoint": "http://172.27.64.1:11434"
  }
}
```

If you prefer to use the community Ollama hosting package and have Aspire manage the lifecycle, refer to the `CommunityToolkit.Aspire.Hosting.Ollama` package documentation for the `AddOllama` API. Note that for this setup (Ollama on the Windows host, not in a container) the external connection string approach above is more appropriate.

---

## Connectivity

### Finding the host IP from the VM

The Hyper-V Default Switch gives the Windows host a virtual NIC. From the VM:

```bash
ip route show default
# The gateway IP is the host's address on the Default Switch
```

Alternatively, on the Windows host:

```powershell
ipconfig
# Look for "Ethernet adapter vEthernet (Default Switch)"
# The IPv4 address there is what the VM uses to reach the host
```

A typical address is in the `172.x.x.0/28` range, but Hyper-V reassigns it on each host reboot. For a stable address, create a dedicated Hyper-V Internal Switch and assign a static IP to the host vNIC.

### Windows Firewall

If the VM cannot reach Ollama, add an inbound rule on the host to allow TCP 11434:

```powershell
New-NetFirewallRule `
  -DisplayName "Ollama API (Hyper-V)" `
  -Direction Inbound `
  -Protocol TCP `
  -LocalPort 11434 `
  -Action Allow
```

### Test connectivity from the VM

```bash
# Check the port is reachable
nc -zv <host-ip> 11434

# Check the API responds
curl http://<host-ip>:11434/api/tags
```

---

## MCP (Model Context Protocol)

> **Status: under evaluation — details to be added in a future update.**

For tool-use and agentic capabilities (letting the LLM access external data sources, call APIs, navigate the codebase, etc.) we will likely need **MCP servers**.

**MCP** (Model Context Protocol) is an open standard for connecting LLMs to external tools and data sources. It defines a structured way for a language model to discover and call tools, query knowledge bases, and interact with services — without baking provider-specific logic into the model itself.

Why AlCopilot may need MCP:

- Give the LLM access to the AlCopilot codebase and documentation at query time
- Allow the LLM to query Qdrant directly via a tool call rather than relying on the host application to do retrieval
- Integrate with GitHub (issues, PRs) or other developer tools
- Enable a more agentic workflow where the LLM orchestrates multi-step tasks

Candidate MCP servers:

| Server | Purpose |
|--------|---------|
| Filesystem MCP | Read project files and documentation |
| GitHub MCP | Access issues, PRs, repository context |
| Qdrant MCP | Semantic search via tool calls |
| Custom AlCopilot MCP | Domain-specific tools and business rules |

**Note**: A practical consideration is that Mistral 7B's tool-calling capabilities are more limited than larger models. The initial approach will be straightforward RAG via Qdrant (embed → retrieve → inject into prompt context) without MCP. MCP servers will be introduced incrementally as tool-use requirements become clearer.

This section will be expanded once we decide which MCP servers to integrate and validate that Mistral 7B handles tool calls reliably enough for the target workflows.
