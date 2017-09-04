# Nord Pool Intraday API .NET Example Code #

This repository contains a .NET (Framework 4.6.1) console client example for interaction with Nord Pool Intraday Trading platform. The respective documentation is located at [our Development Portal](https://developers.nordpoolgroup.com/v1.0/docs/id-introduction). 
This sample code has been created based on the [Java Example Code](https://bitbucket.org/nordpoolspot/public-intraday-api-example) which should be used as primary example if possible. 

## Disclaimer ##

We offer the client code examples to aid the development against Nord Pool's API at no warranty whatsoever. Clients are solely responsible for separately testing and ensuring that interaction with Nord Pool works according to their own standards.

Additionally, we do not supply .NET library of communication protocol objects. We currently maintain [Java API library](https://bitbucket.org/nordpoolspot/public-intraday-api) as our single source of truth. This example does feature 
Order Request protocol object for demo purposes, but to avoid confusion always use the Java API library as your only source for actual objects. 

## Using the example ##

All the relevant variables for connecting are located in App.config. Before running the example, user credentials should be updated to App.config:
```
#!
<add key="sso-user" value="your_username" />
<add key="sso-password" value="your_password" />
```
These credentials shall be obtained from [idapi@nordpoolgroup.com](mailto:idapi@nordpoolgroup.com) prior to running the example.

Additionally, make sure that all the other variabels in the App.config point to correct addresses.
Finally, build the solution (preferably with VS2017) and run it.

The program will connect to the platform and subscribe to several topics. It also will send an invalid order so an error reply will come back from the system. The data received from the system will be printed as JSON in the console. Please note that for clarity we truncate some output that is too large (over 500 chars). 

The sequence of actions are located in **Program.cs** source code.

#Important considerations#

The current program is using the WebSocket4Net and Stomp.Net library to create a StompConnector that can operate through web sockets and handle all SockJS related details. That connector can be found from **Service/Connection/StompConnector.cs**.

The example uses port 8083 for establishing the web socket connection. For some organizations, it maybe so that only ports (80,443) used for HTTP and HTTPS protocols are opened by default. If the example doesn't connect to the API, check that the port 8083 has been opened from your firewall.

## Questions, comments and error reporting ##

Please send questions and bug reports to [idapi@nordpoolgroup.com](mailto:idapi@nordpoolgroup.com).