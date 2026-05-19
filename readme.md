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

A .NET 10 console application that connects to the Nord Pool Intraday Binary PMD v2 API over WebSockets using Protocol Buffers.

No GitHub Packages authentication is required — uses only public NuGet packages.

**Configure** `Username` and `Password` in the `BinaryApi` section of [appsettings.json](NPS.ID.PublicApi.BinaryApiClient/appsettings.json) (or create `appsettings.local.json` to override without modifying the tracked file).

**Build & run:**

```
dotnet build NPS.ID.PublicApi.BinaryApiClient
dotnet run --project NPS.ID.PublicApi.BinaryApiClient
```

Once connected, the client presents an interactive console. Type `help` for available subscription commands. Press `Ctrl+C` to disconnect.

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
