# Nord Pool Intraday API .NET Example Code

Example applications for the [Nord Pool Intraday Trading platform](https://developers.nordpoolgroup.com/v1.0/docs/id-introduction). A Java example is available at [public-intraday-api](https://github.com/NordPool/public-intraday-api).

> **Disclaimer:** Client code examples are provided without warranty. Clients are solely responsible for testing and ensuring correct interaction with Nord Pool.

This repository contains two projects (solution file: [NPS.ID.PublicApi.Example.sln](NPS.ID.PublicApi.Example.sln)):

| Project | Transport | .NET SDK |
|---------|-----------|----------|
| NPS.ID.PublicApi.DotNet.Client | STOMP over WebSocket | .NET 8 |
| NPS.ID.PublicApi.BinaryApiClient | Binary Protobuf over WebSocket | .NET 10 |

Download SDKs from https://dotnet.microsoft.com/en-us/download/dotnet.

---

## STOMP Client (NPS.ID.PublicApi.DotNet.Client)

Uses .NET data objects from the [.NET API library](https://github.com/NordPool/public-intraday-net-api) (requires [GitHub Packages authentication](#authenticating-to-github-packages)).

**Configure** credentials in [appsettings.json](NPS.ID.PublicApi.DotNet.Client/appsettings.json):

```json
"Credentials": {
    "Username": "your_user",
    "Password": "your_password"
}
```

**Build & run:**

```
dotnet build NPS.ID.PublicApi.DotNet.Client
dotnet run --project NPS.ID.PublicApi.DotNet.Client
```

The program connects to both **Market Data** and **Trading** web services, subscribes to example topics, and demonstrates order message sending. See [ApplicationWorker.cs](NPS.ID.PublicApi.DotNet.Client/ApplicationWorker.cs) for the sequence of actions.

The WebSocket connector with heartbeat and token refresh logic is in [WebSocketConnector.cs](NPS.ID.PublicApi.DotNet.Client/Connection/WebSocketConnector.cs). Heartbeat interval is configurable via the **HeartbeatOutgoingInterval** property in appsettings.json. Port 443 must be open in your firewall.

---

## Binary API Client (NPS.ID.PublicApi.BinaryApiClient)

A .NET 10 console application that connects to the Nord Pool Intraday **Binary PMD v2** (public market data) and **Binary Trading v2** APIs over WebSockets using Protocol Buffers. Both connections are opened at start-up and reconnect independently; a single interactive console drives them. Console output is prefixed with `[PMD]` or `[TRD]` to indicate the source connection.

No GitHub Packages authentication is required — uses only public NuGet packages. The proto schemas are in [Proto/](NPS.ID.PublicApi.BinaryApiClient/Proto/) and are compiled at build time.

**Configure** the `BinaryApi` section of [appsettings.json](NPS.ID.PublicApi.BinaryApiClient/appsettings.json) (or create `appsettings.local.json` to override without modifying the tracked file):

| Setting | Description |
|---------|-------------|
| `Pmd:Host` | PMD WebSocket URL template; `{0}` is replaced with a generated session id |
| `Trading:Host` | Trading WebSocket URL template; `{0}` is replaced with a generated session id |
| `Trading:AutoHibe` | Adds `?autoHibe=true` — the server deactivates all orders if no client heartbeat arrives for 10 s. The client then sends a heartbeat every second automatically |
| `Trading:AbortExisting` | Adds `abortExisting=true` on **reconnect** attempts only, aborting a stale session left after an unclean disconnect |
| `SsoUrl`, `ClientId`, `ClientSecret` | SSO token endpoint and client used for both connections |
| `Username`, `Password` | Your API credentials |


**Build & run:**

```
dotnet build NPS.ID.PublicApi.BinaryApiClient
dotnet run --project NPS.ID.PublicApi.BinaryApiClient
```

Once connected, the client presents an interactive console. Type `help` for the full command list. Press `Ctrl+C` or type `quit` to disconnect.

**Public market data commands** (PMD connection): `lvsub`/`lvunsub <areas>`, `dasub`/`daunsub`, `csub`/`cunsub`, `tsub`/`tunsub`, `pssub`/`psunsub <areas>`, `h2hsub`/`h2hunsub <area[:conn,...]>`, `atcsub`/`atcunsub <area[:conn,...]>`.

**Trading commands** (Trading connection):

| Command | Description |
|---------|-------------|
| `oersub [n]` / `oerunsub` | Order execution reports (optional snapshot size). Populates the local order cache used by `modify`/`deac` |
| `ptsub [n]` / `ptunsub` | Private trades (optional snapshot size) |
| `tlsub` / `tlunsub` | Throttling limits |
| `cfgsub` / `cfgunsub` | Configuration / user profile (portfolios, areas). Required before `order` can auto-pick a portfolio |
| `order [key=value ...]` | Enter an order. By default picks a random tradable contract (from `csub`) and a writable portfolio (from `cfgsub`) and sends **SELL LIMIT GFS qty=3000 price=2500**. Override with `side= qty= price= contract= portfolio= area= market= tif= expire=<minutes> state= text=` |
| `modify <id> [key=value ...]` | Modify a cached order — `<id>` is an `order_id`, `client_order_id` or `last`. Unchanged fields are restated from the cached report; override `qty= price= tif= expire= text=` |
| `deac <id>` | Deactivate a cached order |
| `badorder` | Send an intentionally invalid order entry to demonstrate `TradingAck` validation errors |
| `badmodify` | Send a modify for a non-existent order to demonstrate `ITEM_NOT_FOUND` |
| `hb [pmd\|trd]` | Send a heartbeat (both connections by default) |

Every trading request carries a `request_id`; the server echoes it on the resulting `TradingAck`, snapshot or `Error` frame so the console output can be correlated. Errors are printed in red with their code and `is_fatal` flag; fatal errors are followed by a server-side close and an automatic reconnect.


---
## Authenticating to GitHub Packages

Required only for the STOMP client. See [GitHub docs](https://docs.github.com/en/packages/learn-github-packages/introduction-to-github-packages#authenticating-to-github-packages).

Create a personal access token (classic) with `read:packages` scope, then add to NuGet.config:

```xml
<packageSourceCredentials>
    <GitHub>
        <add key="Username" value="github_username" />
        <add key="ClearTextPassword" value="github_access_token" />
    </GitHub>
</packageSourceCredentials>
```

---

## Questions, comments and error reporting

Please send questions and bug reports to [idapi@nordpoolgroup.com](mailto:idapi@nordpoolgroup.com).
