﻿/*
 *  Copyright 2017 Nord Pool.
 *  This library is intended to aid integration with Nord Pool’s Intraday API and comes without any warranty. Users of this library are responsible for separately testing and ensuring that it works according to their own standards.
 *  Please send feedback to idapi@nordpoolgroup.com.
 */

namespace NPS.ID.PublicApi.Client.Security
{
    public class SSOSettings
    {
        public string Host { get; set; }
        public string TokenUri { get; set; }

        public string Protocol { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }
    }
}