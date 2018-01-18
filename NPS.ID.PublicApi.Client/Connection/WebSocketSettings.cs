/*
 *  Copyright 2017 Nord Pool.
 *  This library is intended to aid integration with Nord Pool’s Intraday API and comes without any warranty. Users of this library are responsible for separately testing and ensuring that it works according to their own standards.
 *  Please send feedback to idapi@nordpoolgroup.com.
 */

namespace NPS.ID.PublicApi.Client.Connection
{
    public class WebSocketSettings
    {
        public bool UseSsl { get; set; }

        public int Port { get; set; }

        public string Host { get; set; }

        public string Uri { get; set; }

        public int SslPort { get; set; }

        public int GetUsedPort()
        {
            return UseSsl ? SslPort : Port;
        }
    }
}