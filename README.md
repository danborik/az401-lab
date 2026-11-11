# az401-lab

[![Board Status](https://dev.azure.com/DanielBorik/69a49705-ce11-497b-bbd3-8a7904e3cc7a/25cf2100-dee7-4c2a-9894-537a10ff733e/_apis/work/boardbadge/ae0087e8-e814-4325-9669-4cd3670ad048)](https://dev.azure.com/DanielBorik/69a49705-ce11-497b-bbd3-8a7904e3cc7a/_boards/board/t/25cf2100-dee7-4c2a-9894-537a10ff733e/Stories/)

Small .NET 10 web API used for the AZ-401 study labs. It has a shared library
(packaged in lab 3.6), unit and integration tests with coverage, Bicep for App
Service with a staging slot, a Dockerfile, and endpoints built to be broken on
purpose for the monitoring labs.

## Prerequisites

.NET 10 SDK (`dotnet --version` shows 10.0.x). `global.json` pins the 10.0 SDK band.

## Run locally

```bash
dotnet test                      # 25 unit and integration tests
dotnet run --project src/Api     # then open the URL it prints, e.g. http://localhost:5xxx/
```

On first restore, `packages.lock.json` files are created. Commit them.

## Endpoints

| Endpoint | Used in | What it does |
| --- | --- | --- |
| `/health` | 3.9, 3.11 | Health probe (`Healthy`) |
| `/version` | 3.9 | Running version; override with `dotnet publish /p:Version=1.2.3` |
| `/greet?name=Dan` | 3.10 | Text changes when feature flag `NewGreeting` is on |
| `/weather?city=Prague&days=5` | 5.5-5.7 | Fake forecast; fails randomly when `Chaos__FailRate` > 0 |
| `/weather-proxy` | 5.3 | Calls httpbin.org, so App Insights shows a dependency |
| `/slow?ms=2000` | 5.3, 5.4 | Slow response for performance analysis |
| `/error` | 5.3-5.7 | Always throws (HTTP 500) |
| `/config` | 4.3 | Shows `Settings:Message` and whether `DbPassword` is set |

## Settings that switch lab features on

| App setting | Effect |
| --- | --- |
| `AppConfig__Endpoint` | Load config and feature flags from Azure App Configuration (managed identity) |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | Send telemetry to Application Insights (OpenTelemetry) |
| `FeatureManagement__NewGreeting` | Local feature flag when App Configuration isn't used |
| `Chaos__FailRate` | 0-1, share of `/weather` calls that return 500 |
| `CRASH_ON_STARTUP` | `true` makes the app fail at startup (SRE Agent lab 5.6) |
| `DbPassword` | Set as a Key Vault reference in lab 4.3 |

## Layout

```
src/Api/            web API (minimal APIs)
src/Lab.Common/     shared library, NuGet package Az401.Lab.Common
tests/Api.Tests/    xUnit unit + integration tests, coverlet coverage
infra/main.bicep    App Service plan S1 + web app + staging slot (.NET 10)
reference/          reference pipelines for lab 3.1 (not active)
Dockerfile          container image for container labs
```

## Push to both platforms

```bash
git init -b main && git add . && git commit -m "Initial lab app"
git remote add origin https://github.com/<user>/az401-lab.git
git remote add ado https://dev.azure.com/<org>/az401-lab/_git/az401-lab
git push -u origin main
git push ado main
```
