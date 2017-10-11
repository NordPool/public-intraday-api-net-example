# Nord Pool Intraday API .NET Example Code #

This repository contains a .NET (Framework 4.6.2) Windows Forms example application for interaction with Nord Pool Intraday Trading platform. The respective documentation is located at [our Development Portal](https://developers.nordpoolgroup.com/v1.0/docs/id-introduction). 
This sample application uses .NET data objects published in [.NET API library](https://bitbucket.org/nordpoolspot/public-intraday-net-api)

## Disclaimer ##

We offer the client code examples to aid the development against Nord Pool's API at no warranty whatsoever. Clients are solely responsible for separately testing and ensuring that interaction with Nord Pool works according to their own standards.

## Using the example ##

Example application can be opened with solution file: **NPS.ID.PublicApi.Client.WinFormsExample.sln** which is found in repository root.

Example application requires, that you have cloned [.NET API library](https://bitbucket.org/nordpoolspot/public-intraday-net-api) to your filesystem besides to this example app. Your local repository configuration can be for example:
```
#!
C:\[path]\public-intraday-net-api
C:\[path]\public-intraday-api-net-example
```

All the relevant variables for connecting are located in App.config. Before running the example, user credentials should be updated to App.config:
```
#!
<add key="sso-user" value="your_username" />
<add key="sso-password" value="your_password" />
```
These credentials shall be obtained from [idapi@nordpoolgroup.com](mailto:idapi@nordpoolgroup.com) prior to running the example.

Additionally, make sure that all the other variabels in the App.config point to correct addresses.
Finally, build the solution with VS2017 and run it with startup project: **NPS.ID.PublicApi.Client.WinFormsExample**.

The program will connect to the platform and subscribe to several topics. It also provides examples on sending order messages to Intraday platform.

The sequence of actions are located in **MainForm.cs** source code.

#Important considerations#

The current program is using the WebSocket4Net and Stomp.Net library to create a StompConnector that can operate through web sockets and handle all SockJS related details. That connector can be found from **Service/Connection/StompConnector.cs**.

The example uses port 8083 for establishing the web socket connection. For some organizations, it maybe so that only ports (80,443) used for HTTP and HTTPS protocols are opened by default. If the example doesn't connect to the API, check that the port 8083 has been opened from your firewall.

## Questions, comments and error reporting ##

Please send questions and bug reports to [idapi@nordpoolgroup.com](mailto:idapi@nordpoolgroup.com).