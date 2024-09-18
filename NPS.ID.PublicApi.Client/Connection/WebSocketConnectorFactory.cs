using Microsoft.Extensions.Logging;
using NPS.ID.PublicApi.Client.Connection.Events;
using NPS.ID.PublicApi.Client.Connection.Options;
using NPS.ID.PublicApi.Client.Security;
using NPS.ID.PublicApi.Client.Security.Options;

namespace NPS.ID.PublicApi.Client.Connection;

public class WebSocketConnectorFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ISsoService _ssoService;

    public WebSocketConnectorFactory(
        ILoggerFactory loggerFactory,
        ISsoService ssoService)
    {
        _loggerFactory = loggerFactory;
        _ssoService = ssoService;
    }

    public WebSocketConnector Create(
        WebSocketOptions webSocketOptions, 
        CredentialsOptions credentialsOptions, 
        Func<MessageReceivedEventArgs, CancellationToken, Task> messageReceivedCallbackAsync,
        Func<Task> connectionEstablishedCallbackAsync = null,
        Func<Task> connectionClosedCallbackAsync = null,
        Func<Exception, Task> stompErrorCallbackAsync = null)
    {
        return new WebSocketConnector(
            _loggerFactory.CreateLogger<WebSocketConnector>(), 
            _ssoService, 
            webSocketOptions, 
            credentialsOptions,
            messageReceivedCallbackAsync, 
            connectionEstablishedCallbackAsync, 
            connectionClosedCallbackAsync, 
            stompErrorCallbackAsync);
    }
}