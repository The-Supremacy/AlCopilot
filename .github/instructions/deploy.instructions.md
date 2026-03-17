---
applyTo: 'deploy/**'
---

# Deploy Instructions (Infrastructure & Deployment)

## Architecture Reference

Read [docs/architecture.md](../../docs/architecture.md) for infrastructure decisions and rationale.

## Structure

- `deploy/helm/` — Helm charts (future)
- `deploy/flux/` — Flux GitOps manifests (future)
- `deploy/terraform/` — Infrastructure as Code (future)

## Container Publishing

- .NET SDK container support — no Dockerfile needed
- `dotnet publish --os linux --arch x64 /t:PublishContainer` builds and pushes container images
- Container registry: GHCR (`ghcr.io`)
- Images are published by the `release.yml` workflow on new releases

## Infrastructure Decisions

- **AKS** with Free tier SKU, Standard_B-series nodes
- **Flux** as native AKS extension for GitOps
- **Envoy Gateway** instead of Azure Application Gateway (~$130/month savings)
- **Azure Service Bus** with local emulator for development
