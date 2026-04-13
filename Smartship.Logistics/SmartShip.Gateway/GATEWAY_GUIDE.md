# SmartShip Ocelot API Gateway Guide

## What is implemented

1. **Ocelot as gateway core** (`UseOcelot`) with config-first routing (`ocelot.json`).
2. **Multi-service routing** for Identity, Shipment, Tracking, Document, and Admin.
3. **Centralized JWT validation** in gateway (`GatewayJwt` bearer scheme).
4. **Authorization support**:
   - route-level claim check for Tracking (`permissions=tracking.read`)
   - route-level role check for Admin (`role=Admin`)
5. **Rate limiting** per route using `User-Agent` as client key (works for browser/frontend and Swagger calls).
6. **Response caching** for read-heavy routes via Ocelot file cache options.
7. **Request logging & monitoring** with Serilog + global downstream telemetry handler + `/health` endpoint.
8. **Load balancing** using Ocelot `RoundRobin` and optional service-discovery sample via Consul.
9. **Retry/circuit-breaker timeout handling** via Ocelot QoS + Polly integration.
10. **Downstream failure responses** via standardized `ProblemDetails` for 429/502/503/504 and unexpected 500.
11. **CORS** policy configured from app settings (`Cors:AllowedOrigins`).
12. **Swagger aggregation** using `MMLib.SwaggerForOcelot` at `/swagger`.

## Key files

- `Program.cs`: runtime pipeline, JWT, CORS, logging, Ocelot, Swagger UI.
- `ocelot.json`: route/policy configuration.
- `ocelot.SwaggerEndPoints.json`: downstream Swagger aggregation.
- `ocelot.service-discovery.sample.json`: optional Consul-based discovery example.
- `appsettings.json`: `JwtSettings`, `Cors`, and `Serilog` settings.

## Example downstream service JWT config

Use the same issuer/audience/secret in each secured microservice:

```json
{
  "JwtSettings": {
    "Secret": "THIS_IS_SUPER_SECRET_KEY_FOR_SMARTSHIP_PROJECT_VISHWAJEET",
    "Issuer": "SmartShipAPI",
    "Audience": "SmartShipClient"
  }
}
```

## Run order for local development

1. Run VS Code task: `SmartShip: Start Full Stack`.
2. Open gateway docs: `http://localhost:7000/swagger/index.html`.
3. Open frontend: `http://localhost:4200`.
4. Stop all with task: `SmartShip: Stop Full Stack`.

## Interview quick explanation

- **Why gateway auth?** One place to validate JWT keeps security consistent and avoids duplicated token checks.
- **Why rate limit?** Prevents abuse and protects downstream services from spikes.
- **Why QoS/circuit breaker?** Isolates failures so one unstable service does not degrade the whole platform.
- **Why caching?** Reduces repeated reads and latency for GET-heavy endpoints.
- **Why Swagger aggregation?** Gives a single API entry point for consumers and frontend teams.
- **Why service discovery option?** Supports cloud-native scaling without hardcoded hosts.
