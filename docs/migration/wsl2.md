# WSL2 Migration Plan

## Purpose

This document captures the planned migration from the current Hyper-V based local development setup to a clean WSL2 based setup.
The goal is to keep the Windows host lightweight, preserve the ability to reclaim resources quickly for gaming, and simplify day-to-day development through better editor and networking integration.

---

## Goals

- Replace the separate Hyper-V Ubuntu VM with a fresh WSL2-based Linux development environment.
- Keep the Windows host as thin as possible: primarily VS Code, browser, GPU drivers, and WSL.
- Preserve explicit control over resource usage and allow fast shutdown when full host resources are needed.
- Start with the local LLM hosted inside WSL so application services and the model can be stopped together.
- Keep the monorepo workflow compatible with VS Code workspaces and coding-agent access patterns.
- Prefer Podman over Docker Desktop due to licensing and host-footprint concerns.

---

## Target Shape

### Planned developer experience

- Windows hosts the user interface layer: VS Code, browser, terminals, and GPU drivers.
- One primary WSL2 distro acts as the real development machine.
- The repository lives inside the Linux filesystem of that distro, not under `C:\`.
- VS Code opens the repository through the WSL extension rather than through SSH.
- `dotnet`, `node`, `pnpm`, `git`, Aspire, and Podman run inside WSL.
- The LLM runtime also starts inside WSL during the first migration phase.

### Why this shape

This keeps the day-to-day mental model simple:

- code lives in WSL
- tooling runs in WSL
- containers run in WSL
- the editor connects into WSL
- shutdown is a single WSL-level operation

This is intentionally closer to "one embedded Linux dev box" than to "Windows plus several partially connected runtimes."

---

## Trade-Off Summary

| Topic                     | Hyper-V VM today                  | WSL2 target                                     |
| ------------------------- | --------------------------------- | ----------------------------------------------- |
| Isolation                 | Stronger separation               | Less isolated, more integrated                  |
| Networking                | More manual and VM-like           | Easier `localhost`-style workflows              |
| VS Code workflow          | SSH / remote-machine feeling      | Native-feeling WSL integration                  |
| GPU access for local LLMs | Awkward for Linux guest workloads | Better path for host GPU-backed Linux workloads |
| Shutdown                  | Explicit VM power off             | `wsl --shutdown`                                |
| Host cleanliness          | Very strong                       | Good, but not as invisible as a separate VM     |
| Mental model              | Separate machine                  | Lightweight integrated Linux machine            |

The migration accepts a small loss in isolation in exchange for better usability, easier networking, and a better path for local GPU-backed AI development.

---

## Core Decisions

### 1. WSL becomes the primary local dev machine

The long-term setup should use a single main distro as the canonical place for the repo and the runtime.
This avoids the confusion of having code in one environment and containers or tools in another.

### 2. Start with the LLM inside WSL

The first migration phase keeps the LLM inside WSL.
This matches the shutdown goal well because application processes, containers, and the model runtime can all be stopped together by shutting down WSL.

If GPU support, runtime stability, or tooling friction later proves better on native Windows, the LLM can be moved back to the host without changing the main WSL-first application workflow.

### 3. Prefer Podman over Docker Desktop

Podman aligns better with the current licensing and host-footprint preferences.
Aspire can target Podman, so the migration should avoid introducing Docker Desktop unless a later concrete blocker appears.

### 4. Keep VS Code workspaces as the default navigation model

The repository already uses workspace splitting to help humans and coding agents focus on relevant areas while still preserving monorepo context.
That model remains a better default than moving the whole team into dev containers immediately.

---

## Recommended Architecture

```text
Windows Host
- VS Code
- Browser
- GPU drivers
- WSL2

WSL2 Ubuntu Distro
- AlCopilot repo in ~/src/AlCopilot
- .NET SDK
- Node / pnpm
- Podman
- Aspire AppHost
- Ollama or other local LLM runtime
- Optional vector database container
```

### Expected workflow

1. Open Windows.
2. Launch the WSL distro.
3. Open the repo in VS Code through the WSL extension.
4. Run Aspire, frontend, backend, Podman containers, and the LLM from WSL.
5. When done, stop running processes and execute `wsl --shutdown`.

This makes "give the PC back to games" an ordinary part of the workflow instead of a special-case cleanup operation.

---

## Filesystem Strategy

### Store the repo inside WSL

The monorepo should live inside the Linux filesystem, for example:

```bash
mkdir -p ~/src
git clone <repo-url> ~/src/AlCopilot
```

Recommended path:

- `~/src/AlCopilot`

Avoid placing the working copy under `/mnt/c/...` for normal development.
Linux-side tools perform better and behave more predictably when they operate on files stored inside the WSL filesystem.

### Why this matters

- better filesystem performance for `git`, `node_modules`, package managers, and build tools
- fewer permission and file-watching surprises
- simpler mental model for CLI tools and coding agents

---

## Editor And Workspace Strategy

### Keep VS Code on Windows, connect into WSL

The preferred setup is:

- install VS Code on Windows
- install the WSL extension
- open the repo folder from inside the WSL context

This preserves the convenience of the Windows UI while making Linux the execution environment.

### Keep multi-root workspaces

The existing use of VS Code workspaces remains useful after the migration.
Multi-root workspaces still provide:

- a natural way to split the monorepo by concern
- easier focus for coding agents
- enough global context for cross-cutting work when the workspace includes the relevant roots

Recommended default:

- keep the repository as one checkout in WSL
- keep existing workspace files or add WSL-oriented workspace variants if needed
- continue using sub-workspaces to reduce cognitive load, not to artificially isolate codebases

### Dev containers: should we use them?

Not as the default first step.

Dev containers are worth reconsidering later if one of these becomes painful:

- onboarding repeatedly drifts because local tool versions differ too much
- the team needs stronger reproducibility across contributors
- local runtime dependencies become too messy to manage directly in WSL

For the current goals, dev containers add an extra indirection layer:

- containerized editor/runtime expectations
- more volume and mount behavior to reason about
- more overlap with the existing "one WSL distro as the dev machine" concept

Current recommendation:

- use WSL as the primary dev environment
- use Podman containers for app dependencies and supporting services
- keep VS Code workspaces for repo navigation
- postpone dev containers unless a clear consistency problem appears

In short, WSL plus workspace-based monorepo navigation is the more natural and lower-friction modern setup for this repository right now.

---

## Container Runtime Plan

### Initial direction

- install Podman inside the main WSL distro
- run Aspire and supporting services from the same distro
- avoid Docker Desktop on Windows

This keeps the model simple:

- one Linux environment
- one shell context
- one place to inspect processes, logs, and volumes

### Why avoid an extra managed container machine

Using a desktop container app that creates a separate hidden machine can work, but it splits responsibilities:

- one distro for code
- another environment for containers

That is acceptable for convenience, but it is not the cleanest long-term shape.
The preferred long-term approach is that the same WSL distro owns both the code and the container runtime.

---

## LLM Hosting Plan

### Phase 1: host the LLM inside WSL

The initial plan is to run the local LLM inside WSL alongside the rest of the development stack.

Benefits:

- one shutdown boundary
- simpler mental model
- easier to reason about local service dependencies
- no split between "app machine" and "model machine"

Expected layout:

- backend app in WSL
- frontend dev server in WSL
- Podman-managed dependencies in WSL
- LLM runtime in WSL

### Phase 2 fallback: move the LLM to Windows if needed

If the Linux-side runtime is harder to operate than expected, the fallback is to keep the application stack in WSL and move only the LLM back to native Windows.
This is an allowed optimization, not a failure of the migration.

That fallback would be justified if:

- GPU support is materially better on Windows
- driver/runtime issues appear under WSL
- model startup or performance is significantly more reliable on the host

---

## Resource And Shutdown Model

### Expected operating model

The WSL setup must preserve "easy on, easy off" behavior.

Normal work session:

- start WSL
- run the needed processes
- work normally

Gaming or full-resource session:

- stop app processes if needed
- execute `wsl --shutdown`

This should release CPU and memory used by WSL-backed workloads.

### Configure resource caps

Add a user-level `%UserProfile%\\.wslconfig` file on Windows to limit how much RAM and CPU WSL can consume.
Example values should be tuned after observing actual local usage.

Illustrative example:

```ini
[wsl2]
memory=16GB
processors=8
swap=8GB
vmIdleTimeout=60000

[experimental]
autoMemoryReclaim=gradual
```

The exact numbers should match the host machine and actual AI workload requirements.

---

## Migration Steps

### Phase 0: finish current Hyper-V-based work

Before changing environments:

- finish the current task
- push current work to GitHub
- preserve any local-only notes or scripts that need to survive the migration

### Phase 1: prepare the Windows host

1. Enable or verify WSL2 support.
2. Install the chosen Linux distro, likely Ubuntu.
3. Install or verify VS Code on Windows.
4. Install the VS Code WSL extension.
5. Decide initial `.wslconfig` resource limits.

### Phase 2: create the clean WSL distro

1. Launch the distro and create the primary user.
2. Update packages.
3. Install baseline tooling:
   - `git`
   - `curl`
   - `build-essential`
   - `.NET SDK`
   - `Node.js`
   - `pnpm`
   - `Podman`
4. Create the source root, for example `~/src`.
5. Clone `AlCopilot` into `~/src/AlCopilot`.

### Phase 3: restore development workflow

1. Open the repo in VS Code through WSL.
2. Restore any existing workspace files or create WSL-friendly workspace entry points if needed.
3. Verify `git`, `dotnet`, `node`, and `pnpm` from the WSL terminal.
4. Run the backend and frontend locally without the LLM first.

### Phase 4: restore container-backed dependencies

1. Configure Podman in WSL.
2. Start required service containers.
3. Verify Aspire can target Podman correctly.
4. Confirm normal local development flows work without Hyper-V.

### Phase 5: add the LLM inside WSL

1. Install the selected local LLM runtime inside WSL.
2. Pull the initial model.
3. Run the model locally in WSL.
4. Verify the application can reach the model endpoint.
5. Check memory, CPU, and GPU behavior during inference.

### Phase 6: validate shutdown and restart behavior

1. Stop active processes.
2. Run `wsl --shutdown`.
3. Confirm host resources are reclaimed as expected.
4. Restart WSL and verify the environment resumes cleanly.

---

## Validation Checklist

- The repo is stored in the WSL filesystem.
- VS Code opens the repo through WSL, not SSH and not `/mnt/c`.
- `dotnet build` works in WSL.
- frontend install and dev server work in WSL.
- Podman runs in WSL.
- Aspire works against the chosen runtime.
- The LLM can be started and stopped from WSL.
- `wsl --shutdown` cleanly stops the environment.
- The host remains comfortable for non-dev usage after shutdown.

---

## Replicating Or Snapshotting WSL Environments

### Is there a snapshot concept?

Yes, functionally.
WSL does not feel exactly like Hyper-V checkpoints, but it does support export and import workflows that are good enough for reproducible baseline environments.

Practical options:

- export a distro to a `.tar`
- export a distro to a `.vhdx`
- import that image as a new distro
- duplicate a prepared "golden" distro for similar machines

This supports a useful pattern:

1. create a clean baseline distro
2. install core tooling
3. export it
4. reuse it to create additional similar WSL machines later

### Recommended use of snapshots

Use snapshots for baseline operating-system and tool bootstrapping, not as a replacement for normal repository setup.

Good candidates:

- base Linux packages
- SDKs
- shell tooling
- Podman and shared CLI dependencies

Avoid baking in:

- live feature branches
- local secrets
- large transient caches unless intentionally desired

### Practical strategy

- maintain one clean "base-dev" distro image
- create project-specific distros only if there is a real need
- otherwise keep one main distro and rely on Git plus scripts for most reproducibility

For this repository, one primary distro is likely enough unless future isolation or experimental setups justify more.

---

## Open Questions To Revisit After Migration

- Does the chosen LLM runtime in WSL use the host GPU reliably enough for daily development?
- Are Podman and Aspire smooth enough inside one distro, or is a desktop management layer still useful?
- Do the existing VS Code workspaces need WSL-specific path updates?
- Does the team eventually need dev containers for stricter onboarding consistency?

These are follow-up validation questions, not blockers for the migration.

---

## Recommended Default Outcome

The desired steady state is:

- one main WSL2 distro
- repo stored inside WSL
- VS Code connected through WSL
- Podman running inside the same distro
- LLM running inside the same distro initially
- `wsl --shutdown` used as the primary "give my PC back" command

This keeps the environment close to the simplicity of a dedicated VM while reducing the networking and editor friction that currently comes with Hyper-V.
