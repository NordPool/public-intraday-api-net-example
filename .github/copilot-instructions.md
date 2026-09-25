# Copilot Instructions for public-intraday-api-net-example

## What this repo is

Example/reference **console client applications** for the Nord Pool Intraday
Trading platform (external market data & trading API). This is **not a
production library** — it's disclaimer-covered sample code showing how to
connect, authenticate, subscribe to topics, and send orders. Don't add
production-hardening (retry frameworks, DI abstractions, test suites, etc.)
unless explicitly asked; keep changes in the spirit of a readable example.

Two independent console apps, one solution (`NPS.ID.PublicApi.Example.sln`):

| Project | Folder | Transport | .NET SDK |
|---|---|---|---|
| STOMP client | `NPS.ID.PublicApi.Client` | STOMP over WebSocket (text protocol) | net8.0 |
| Binary API client | `NPS.ID.PublicApi.BinaryApiClient` | Protobuf over WebSocket (binary) | net10.0 |

They share no code and can be built/run independently.

> ⚠️ There is a stray, git-ignored `NPS.ID.PublicApi.DotNet.Client/` directory
> at the repo root containing only leftover `bin`/`obj` output from an older
> project name. It is **not** part of the solution — ignore it; do not add
> source files there.

## STOMP client — `NPS.ID.PublicApi.Client`

- `Program.cs` wires up a generic host (`Host.CreateApplicationBuilder`) with
  DI configured in `ServiceCollectionExtensions.cs`.
- `ApplicationWorker.cs` is the orchestration entry point: connects to both
  **Market Data** and **Trading** WebSocket services, subscribes to example
  topics (delivery areas, configurations, order execution reports, etc.), and
  demonstrates sending order messages. Read this file first to understand the
  demo's call sequence.
- `Connection/WebSocketConnector.cs` owns the raw `ClientWebSocket`, the STOMP
  frame handshake, periodic heartbeats (`HeartbeatOutgoingInterval` from
  config), and **automatic JWT refresh** (refreshes ~5 minutes before token
  expiry via `PeriodicallyRefreshTokenAsync`, sending a `TOKEN_REFRESH`
  command over the existing connection instead of reconnecting).
- `Security/SsoService.cs` fetches the JWT via OAuth2 password grant against
  the `Sso` endpoint in config.
- Depends on the **`NPS.ID.PublicApi.Models`** NuGet package published to
  **GitHub Packages** (source: `nuget.Config`). Building this project requires
  a PAT with `read:packages` scope configured via `nuget.Config`'s
  `packageSourceCredentials` (or a user-level NuGet config) — without it,
  restore/build fails.
- Config: `appsettings.json` holds `Credentials` (Username/Password), `Sso`
  (endpoint/client id/secret), and per-service `Endpoints` (host/port/heartbeat
  interval). There is no local-override file for this project.

Build & run:
```
dotnet build NPS.ID.PublicApi.Client
dotnet run --project NPS.ID.PublicApi.Client
```

## Binary API client — `NPS.ID.PublicApi.BinaryApiClient`

- Plain top-level-statement `Program.cs` (no generic host/DI) that loads
  config, validates required settings, then constructs `SsoClient` +
  `BinaryApiClient` directly.
- `BinaryApiClient.cs` opens a `ClientWebSocket`, handles reconnect/backoff,
  token refresh, an interactive console command loop (type `help` for
  commands; `Ctrl+C` disconnects), and Subscribe/Unsubscribe helpers per topic
  (local view, delivery area, contract, ticker, statistics, capacity).
- `ConsoleIO.cs` is a small thread-safe helper that keeps the interactive
  input prompt visually separate from async output being printed concurrently
  — reuse it rather than calling `Console.WriteLine` directly when adding new
  interactive output.
- `Proto/pmd.api.v2.proto` defines the wire format (`ServerFrame`/`ClientFrame`
  `oneof` messages). It's compiled via `Grpc.Tools` with `GrpcServices="None"`
  in the `.csproj` — **proto-only C# messages, no gRPC service stubs**. After
  editing the `.proto`, just rebuild; codegen runs automatically at build
  time (generated code is not checked in).
- No GitHub Packages auth needed — only public NuGet packages.
- Config loading (`Program.cs`): `appsettings.json` (required) is layered with
  `appsettings.local.json` (optional, overrides). **Both files are tracked in
  git** with empty `Username`/`Password` placeholders — `appsettings.local.json`
  is a convention for keeping personal edits in a separate file, **not** a
  git-ignored secret store. Never commit real credentials into either file.

Build & run:
```
dotnet build NPS.ID.PublicApi.BinaryApiClient
dotnet run --project NPS.ID.PublicApi.BinaryApiClient
```

## Credentials & secrets — hard rules

- Never commit real usernames/passwords/tokens into `appsettings.json`,
  `appsettings.local.json`, or `nuget.Config`. All committed values are empty
  strings or placeholders (`your_github_username`, `your_github_access_token`)
  — keep it that way in any change you make.
- `ClientId`/`ClientSecret` values already present in the sample configs are
  shared public test-environment identifiers used by the docs/README, not
  per-user secrets.
- If asked to add config, follow the existing options-binding pattern (record
  a `SectionName`, bind via `IOptions<T>` + `ValidateDataAnnotations()` for
  the STOMP client; a plain POCO + `IConfiguration.Get<T>()` for the Binary
  client) rather than reading `IConfiguration` ad hoc.

## Conventions observed

- File-scoped `namespace` declarations, `ImplicitUsings` enabled.
- Async methods suffixed `Async`; cancellation tokens threaded through calls.
- Logging via `ILogger<T>` (STOMP client) — prefer `LogDebug`/`LogWarning`
  over `Console.WriteLine` there. The Binary client has no `ILogger`; it uses
  its own `Log`/`LogError` helpers and `ConsoleIO`.
- No automated test project exists in this repo — do not assume one; verify
  changes with `dotnet build` (and manual run against test endpoints) instead.
