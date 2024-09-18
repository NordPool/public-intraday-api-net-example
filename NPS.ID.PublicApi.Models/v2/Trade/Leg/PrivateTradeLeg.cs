/*
 *  Copyright 2023 Nord Pool.
 *  This library is intended to aid integration with Nord Pool�s Intraday API and comes without any warranty. Users of this library are responsible for separately testing and ensuring that it works according to their own standards.
 *  Please send feedback to idapi@nordpoolgroup.com.
 */

using System;
using Nordpool.ID.PublicApi.v1.Order;

namespace NPS.ID.PublicApi.Models.v2.Trade.Leg
{
    public class PrivateTradeLeg : BaseTradeLeg
    {
        public string UserId { get; set; }

        public string PortfolioId { get; set; }

        public string ClientOrderId { get; set; }

        public OrderType OrderType { get; set; }

        public string OrderId { get; set; }

        public DateTimeOffset DeliveryStart { get; set; }

        public DateTimeOffset DeliveryEnd { get; set; }
        
        public string Text { get; set; }
    }
}