using System;
using System.IO;
using System.Reflection;
using System.Text;
using log4net;
using Newtonsoft.Json;
using Stomp.Net.Stomp.Protocol;
using WebSocket4Net;
using ErrorEventArgs = SuperSocket.ClientEngine.ErrorEventArgs;
using NPS.ID.PublicApi.Client.Connection;

namespace NPS.ID.PublicApi.Client.Connection
{
    /// <summary>
    /// Connector for sending/receiving STOMP messages over SockJS using web sockets.
    /// </summary>
    public class StompConnector
    {
        public delegate void StompConnectionClosedEventHandler(object sender, EventArgs e);

        public delegate void StompConnectionEstablishedEventHandler(object sender, EventArgs e);

        public delegate void StompErrorEventHandler(object sender, StompErrorEventArgs e);

        public delegate void StompFrameReceivedEventHandler(object sender, StompFrameReceivedEventArgs e);

        private static readonly string SockJsStartMessage = "o";
        private static readonly string HeartBeatMessage = "h";


        private static readonly ILog _logger =
            LogManager.GetLogger(
                MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string _hostName;
        private readonly int _port;
        private readonly string _protocol;
        private readonly string _uri;

        private readonly WebSocket _webSocket;

        private string _currentAuthToken;

        public bool IsConnected { get; private set; }
        public string ConnectionUri { get; private set; }
        public event StompFrameReceivedEventHandler StompFrameReceived;
        public event StompConnectionEstablishedEventHandler StompConnectionEstablished;

        public event StompConnectionClosedEventHandler StompConnectionClosed;

        public event StompErrorEventHandler StompError;

        public StompConnector(string hostName, string uri, int port, string protocol = "ws")
        {
            _hostName = hostName;
            _uri = uri;
            _port = port;
            _protocol = protocol;
            _webSocket = new WebSocket(ConstructUri());
        }

        /// <summary>
        /// Constructs Uri that has both serverId and sessionId generated. See more from: https://sockjs.github.io/sockjs-protocol/sockjs-protocol-0.3.3.html#section-36
        /// </summary>
        /// <returns></returns>
        private string ConstructUri()
        {
            var serverId = ConstructServerId();
            var sessionId = Guid.NewGuid().ToString("N");
            var uri = $"{_protocol}://{_hostName}:{_port}{_uri}/{serverId}/{sessionId}/websocket";

            _logger.Info($"Connecting to: {uri}");
            ConnectionUri = uri;
            return uri;
        }

        /// <summary>
        /// SockJS requires client to generate random server id between 000-999
        /// </summary>
        /// <returns></returns>
        private static string ConstructServerId()
        {
            var rnd = new Random();
            var serverId = rnd.Next(0, 999);

            return serverId.ToString().PadLeft(3, '0');
        }

        public void Connect(string authToken)

        {
            _currentAuthToken = authToken;
            _webSocket.Opened += WebSocketOnOpened;
            _webSocket.MessageReceived += WebSocketOnMessageReceived;
            _webSocket.Error += WebSocketOnError;
            _webSocket.Closed += WebSocketOnClosed;
            _webSocket.AllowUnstrustedCertificate = true;
            _webSocket.Open();
        }


        private void WebSocketOnClosed(object sender, EventArgs eventArgs)
        {
            StompConnectionClosed?.Invoke(this, eventArgs);
        }

        private void WebSocketOnError(object sender, ErrorEventArgs errorEventArgs)
        {
            OnStompError(new StompErrorEventArgs
            {
                IsFatal = true,
                Message = errorEventArgs.Exception.ToString()
            });
        }

        private void WebSocketOnMessageReceived(object sender, MessageReceivedEventArgs messageReceivedEventArgs)
        {
            if (messageReceivedEventArgs.Message == SockJsStartMessage)
            {
                Send(StompMessageFactory.ConnectionFrame(_currentAuthToken));
                return;
            }

            // ignore heartbeat messages from the server
            if (messageReceivedEventArgs.Message == HeartBeatMessage)
                return;

            var stompMessage = StompMessageFromSockJSWrapper(messageReceivedEventArgs.Message);
            var bytes = Encoding.ASCII.GetBytes(stompMessage);
            var frame = FrameFromBytes(bytes);

            if (frame.Command == StompCommandConstants.Connected) 
            {
                //Raise connected event after receiving connection
                OnConnected(EventArgs.Empty);
                IsConnected = true;
            }
            else
            {
                OnFrameReceived(new StompFrameReceivedEventArgs() {Frame = frame});
            }
        }

        /// <summary>
        /// Gets the plain STOMP message from a received message. Strips SockJS wrapping.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private string StompMessageFromSockJSWrapper(string message)
        {
            //Remove the first char 'a' to get the json array
            var jsonArrayString = message.Substring(1, message.Length - 1);

            //Deserialize the string to string array and take the first item.
            return JsonConvert.DeserializeObject<string[]>(jsonArrayString)[0];
        }

        private void WebSocketOnOpened(object sender, EventArgs eventArgs)
        {
        }

        public void Disconnect()
        {
        }

        protected virtual void OnFrameReceived(StompFrameReceivedEventArgs e)
        {
            _logger.Info($"====================================");        
            _logger.Warn($"Received new STOMP frame: {e.Frame}");
            var handler = StompFrameReceived;
            handler?.Invoke(this, e);
        }

        protected virtual void OnConnected(EventArgs e)
        {
            var handler = StompConnectionEstablished;
            _logger.Info("STOMP Connection established");
            handler?.Invoke(this, e);
        }

        protected virtual void OnDisconnected(EventArgs e)
        {
            var handler = StompConnectionClosed;
            handler?.Invoke(this, e);
        }

        public void Send(StompFrame frame)
        {
            var bytes = FrameToBytes(frame);

            var stringOfBytes = Encoding.UTF8.GetString(bytes);
            var serializedJsonArray = SerializeToJsonArray(stringOfBytes);

            _logger.Info("Sending stomp frame: ");
            _logger.Info(serializedJsonArray);
            _webSocket.Send(serializedJsonArray);
        }


        private static byte[] FrameToBytes(StompFrame frame)
        {
            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms, Encoding.UTF8))
                {
                    frame.ToStream(bw);

                    return ms.ToArray();
                }
            }
        }

        private static StompFrame FrameFromBytes(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                ms.Position = 0;
                using (var br = new BinaryReader(ms, Encoding.UTF8))
                {
                    var frame = new StompFrame(true);
                    frame.FromStream(br);

                    return frame;
                }
            }
        }

        private static string SerializeToJsonArray(string message)
        {
            return JsonConvert.SerializeObject(new[] {message});
        }

        protected virtual void OnStompError(StompErrorEventArgs e)
        {
            StompError?.Invoke(this, e);
        }

        public class StompFrameReceivedEventArgs : EventArgs
        {
            public StompFrame Frame { get; set; }
        }
    }

    public class StompErrorEventArgs
    {
        public string ErrorCode { get; set; }

        public string Message { get; set; }

        public bool IsFatal { get; set; }
    }
}