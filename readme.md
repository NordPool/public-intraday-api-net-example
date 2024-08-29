# Nord Pool Intraday API .NET Example Code #

This repository contains a .NET 8 Console example application for interaction with Nord Pool Intraday Trading platform. The respective documentation is located at [our Development Portal](https://developers.nordpoolgroup.com/v1.0/docs/id-introduction). 

This sample application uses .NET data objects published in [.NET API library](https://github.com/NordPool/public-intraday-net-api)

There is also sample Java application available at [https://github.com/NordPool/public-intraday-api](https://github.com/NordPool/public-intraday-api). 

## Disclaimer ##

We offer the client code examples to aid the development against Nord Pool's API at no warranty whatsoever. Clients are solely responsible for separately testing and ensuring that interaction with Nord Pool works according to their own standards.

Example application requires .NET 8. If you have not support for .NET 8. in your Visual Studio, install developer SDK from (https://dotnet.microsoft.com/en-us/download/dotnet/8.0).


## Building ##

Example application can be opened with solution file: **NPS.ID.PublicApi.DotNet.Client.sln** which is found in repository root.

Example application requires, that you have cloned [.NET API library](https://github.com/NordPool/public-intraday-net-api) to your filesystem besides to this example app. Your local repository configuration can be for example:
```
#!
C:\[path]\public-intraday-net-api
C:\[path]\public-intraday-api-dotnet-console-example
```

.NET Example client solution contains Api library-project including source code from public-intraday-net-api repository. .NET API library reference is always to the same version of the library than exists in disk.

All the relevant variables for connecting are located in **appsettings.json**. Before running the example, user credentials should be updated to appsettings.json:
```
#!
"Credentials": {
    "Username": "your_user",
    "Password": "your_password"
}
```
These credentials shall be obtained from [idapi@nordpoolgroup.com](mailto:idapi@nordpoolgroup.com) prior to running the example.

Additionally, make sure that all the other variabels in the appsettings.json file point to correct addresses.
Finally, build the solution with Visual Studio or with dotnet CLI and run it with startup project: **NPS.ID.PublicApi.DotNet.Client**.

The program will create two parallel connections that targets both: new **PMD API** web service and old **Middleware** web service. 
Each connection subscribes to several example topics. It also provides examples on sending order messages to Intraday platform.

Every communication step, its results or exceptions are printed in console output window.

The sequence of actions are located in **ApplicationWorker.cs** source code which is triggered once the program has started.

## Important considerations ##

The current program is using the native .NET ClientWebSocket library and Stomp.Net library to create a WebSocketConnector that can operate through web sockets and handle all SockJS related details. In addition, sending heartbeat task created after connection established is also defined in the WebSocketConnector. That connector can be found from **WebSocketConnector.cs**. 
Heartbeat interval configuration can be found in appsettings.json **HeartbeatOutgoingInterval** property.

The example uses ports 8083/443(secured) for establishing the web socket connection with **Middleware** web service and ports 80/443(secured) for establishing web socket connection with **PMD UI** web service. 
If the example doesn't connect to the API, check that the above ports has been opened from your firewall.

## Questions, comments and error reporting ##

Please send questions and bug reports to [idapi@nordpoolgroup.com](mailto:idapi@nordpoolgroup.com).

## SSL configuration: 

Change  useSsl property value from false to true.
```
#!
"Endpoints": {
    "Middleware": {
      "UseSsl": true
    },
    "Edge": {
      "UseSsl": true
    },
}
```
