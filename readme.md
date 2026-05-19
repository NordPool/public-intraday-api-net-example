# Nord Pool Intraday API .NET Example Code #

This repository contains a .NET 8 Console example application for interaction with Nord Pool Intraday Trading platform. The respective documentation is located at [our Development Portal](https://developers.nordpoolgroup.com/v1.0/docs/id-introduction). 

This sample application uses .NET data objects published in [.NET API library](https://github.com/NordPool/public-intraday-net-api)

There is also sample Java application available at [https://github.com/NordPool/public-intraday-api](https://github.com/NordPool/public-intraday-api). 

## Disclaimer ##

We offer the client code examples to aid the development against Nord Pool's API at no warranty whatsoever. Clients are solely responsible for separately testing and ensuring that interaction with Nord Pool works according to their own standards.

Example application requires .NET 8. If you have not support for .NET 8. in your Visual Studio, install developer SDK from (https://dotnet.microsoft.com/en-us/download/dotnet/8.0).


## Building ##

Example application can be opened with solution file: [NPS.ID.PublicApi.DotNet.Client.sln](NPS.ID.PublicApi.DotNet.Client.sln) which is found in repository root.

It requires that you add your github token in nuget.cofig file. For more detailed instruction see **Authenticating to GitHub Packages** section of this document.

All the relevant variables for connecting are located in [appsettings.json](NPS.ID.PublicApi.DotNet.Client/appsettings.json). Before running the example, user credentials should be updated to [appsettings.json](NPS.ID.PublicApi.DotNet.Client/appsettings.json):

```
"Credentials": {
    "Username": "your_user",
    "Password": "your_password"
}
```

Additionally, make sure that all the other variables in the [appsettings.json](NPS.ID.PublicApi.DotNet.Client/appsettings.json) file point to correct addresses.
Finally, build the solution with Visual Studio or with dotnet CLI and run it with startup project: **NPS.ID.PublicApi.DotNet.Client**.

The program will create two parallel connections that targets both: **Market Data** web service and **Trading** web service. 
Each connection subscribes to several example topics. It also provides examples on sending order messages to Intraday platform.

Every communication step, its results or exceptions are printed in console output window.

The sequence of actions are located in [ApplicationWorker.cs](NPS.ID.PublicApi.DotNet.Client/ApplicationWorker.cs) source code which is triggered once the program has started.

## Authenticating to GitHub Packages ##

See more information on [GitHub docs](https://docs.github.com/en/packages/learn-github-packages/introduction-to-github-packages#authenticating-to-github-packages)

GitHub Packages only supports authentication using a personal access token (classic).

You can use a personal access token (classic) to authenticate to GitHub Packages or the GitHub API. You will need `read:packages` scope to be enabled.

Login and access token can be set in NuGet.config file:
```
<packageSourceCredentials>
    <GitHub>
        <add key="Username" value="github_username" />
        <add key="ClearTextPassword" value="github_access_token" />
    </GitHub>
</packageSourceCredentials>
```

## Important considerations ##

The current program is using the native .NET ClientWebSocket library and Stomp.Net library to create a WebSocketConnector that can operate through web sockets and handle all SockJS related details. In addition, sending heartbeat task created after connection established and refreshing access token are also defined in the WebSocketConnector. That connector can be found from [WebSocketConnector.cs](NPS.ID.PublicApi.DotNet.Client/Connection/WebSocketConnector.cs). 
Heartbeat interval configuration can be found in [appsettings.json](NPS.ID.PublicApi.DotNet.Client/appsettings.json) **HeartbeatOutgoingInterval** property.

The example uses port 443(secured) for establishing the web socket connection with **Trading** and **Market Data** web services. 
If the example doesn't connect to the API, check that the above ports has been opened from your firewall.

## Questions, comments and error reporting ##

Please send questions and bug reports to [idapi@nordpoolgroup.com](mailto:idapi@nordpoolgroup.com).
