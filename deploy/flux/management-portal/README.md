# Management Portal Flux Manifests

These manifests define the AKS ingress and service wiring for the management portal.

## Included resources

- `Gateway` listener for `management.alcopilot.com`
- `HTTPRoute` targeting the `management-portal` service
- `SecurityPolicy` attaching Basic Auth to the management route
- `Deployment` and `Service` for the portal runtime

## Temporary access restriction

Application-level access control is intentionally deferred in the current change.
Until that work lands, access is restricted operationally with Envoy Gateway Basic Auth.

Create a real secret from an htpasswd file before applying:

```bash
htpasswd -cbs .htpasswd <user> <password>
kubectl create secret generic management-basic-auth \
  --namespace alcopilot \
  --from-file=.htpasswd
```

The committed `management-basic-auth.secret.example.yaml` file is a placeholder only and should not be applied with dummy credentials.

## Assumptions

- Envoy Gateway is installed with an `envoy` `GatewayClass`
- the portal is deployed as its own HTTP service on port `8080`
- the image name will be published to GHCR as `ghcr.io/oxface/alcopilot/management-portal`

If release automation uses a different repository or tag scheme, update `management-portal.deployment.yaml` accordingly.
