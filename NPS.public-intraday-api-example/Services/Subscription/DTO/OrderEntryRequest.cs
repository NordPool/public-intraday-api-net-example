/*
 *  Copyright 2017 Nord Pool.
 *  This library is intended to aid integration with Nord Pool’s Intraday API and comes without any warranty. Users of this library are responsible for separately testing and ensuring that it works according to their own standards.
 *  Please send feedback to idapi@nordpoolgroup.com.
 */

using System;

namespace NPS.public_intraday_api_example.Services.Subscription.DTO
{

    /// <summary>
    /// NOTE: This class is NOT the official contract for OrderEntryRequest and one should ALWAYS check the required fields from the official documentation: https://developers.nordpoolgroup.com/docs/creating-and-modifying-orders
    /// </summary>
    public class OrderEntryRequest
    {
        public string requestId { get; set; }
        public bool rejectPartially { get; set; }
        public Order[] orders { get; set; }

        public string orderdModificationType { get; set; }

        public class Order
        {
            public string portfolioId { get; set; }
            public string[] contractIds { get; set; }
            public int deliveryAreaId { get; set; }
            public string side { get; set; }
            public long? clipSize { get; set; }
            public long? clipPriceChange { get; set; }
            public string orderType { get; set; }
            public int unitPrice { get; set; }
            public int quantity { get; set; }
            public string timeInForce { get; set; }
            public string executionRestriction { get; set; }
            public DateTime? expireTime { get; set; }
            public string state { get; set; }
            public string clientOrderId { get; set; }
        }
    }
}