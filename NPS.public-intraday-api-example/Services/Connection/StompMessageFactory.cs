/*
 *  Copyright 2017 Nord Pool.
 *  This library is intended to aid integration with Nord Pool’s Intraday API and comes without any warranty. Users of this library are responsible for separately testing and ensuring that it works according to their own standards.
 *  Please send feedback to idapi@nordpoolgroup.com.
 */

using System.Collections.Generic;
using System.Text;
using NPS.public_intraday_api_example.Utilities;
using Stomp.Net.Stomp.Protocol;

namespace NPS.public_intraday_api_example.Services.Connection
{
    public static class StompMessageFactory
    {
        private const string AcceptVersionHeader = "accept-version";
        private const string HeartbeatHeader = "heartbeat";
        private const string AuthHeader = "X-AUTH-TOKEN";
        private const string DestinationHeader = "destination";
        private const string SubscriptionIdHeader = "id";
        private const string ContentTypeHeader = "content-type";
        private const string ContentLengthHeader = "content-length";

        public static StompFrame ConnectionFrame(string authToken)
        {
            return CreateFrame(StompCommmandConstants.Connect, new Dictionary<string, string>()
            {
                {AcceptVersionHeader, "1.1,1.0"},
                {HeartbeatHeader, "0,0"},
                {AuthHeader, authToken}
            });
        }

        public static StompFrame SendFrame(string payload, string destination, string contentType = "application/json;charset=UTF-8")
        {
            return CreateFrame(StompCommmandConstants.Send, new Dictionary<string, string>()
            {
                {ContentTypeHeader, contentType},
                {DestinationHeader, destination},
            }, payload);
        }

        public static StompFrame SubscribeFrame(string destination, string id)
        {
            return CreateFrame(StompCommmandConstants.Subscribe, new Dictionary<string, string>()
            {
                {DestinationHeader, destination},
                {SubscriptionIdHeader, id}
            });
        }

        public static StompFrame Unsubscribe(string destination, string id)
        {
            return CreateFrame(StompCommmandConstants.Unsubscribe, new Dictionary<string, string>()
            {
                {DestinationHeader, destination},
                {SubscriptionIdHeader, id}
            });
        }

        private static StompFrame CreateFrame(string command, Dictionary<string, string> headers, string payload = null,
            bool useGzip = false)
        {
            var frame = new StompFrame(true) {Command = command};


            foreach (var header in headers)
            {
                frame.SetProperty(header.Key, header.Value);
            }

            if (payload != null)
            {
                var contentBytes = Encoding.UTF8.GetBytes(payload);

                if (!useGzip)
                {
                    frame.Content = contentBytes;
                    frame.SetProperty(ContentLengthHeader, contentBytes.Length);
                }
                else
                {
                    var compressedContent = GzipCompressor.Compress(contentBytes);
                    frame.Content = compressedContent;
                    frame.SetProperty(ContentLengthHeader, compressedContent.Length);
                }

            }

            return frame;
        }
    }
}