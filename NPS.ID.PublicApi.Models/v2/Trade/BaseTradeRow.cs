/*
 *  Copyright 2023 Nord Pool.
 *  This library is intended to aid integration with Nord Pool�s Intraday API and comes without any warranty. Users of this library are responsible for separately testing and ensuring that it works according to their own standards.
 *  Please send feedback to idapi@nordpoolgroup.com.
 */

using System;
using Nordpool.ID.PublicApi.v1.Trade;

namespace NPS.ID.PublicApi.Models.v2.Trade
{
    /// <summary>Class with base trade fields</summary>
    public class BaseTradeRow
    {
        public string TradeId { get; set; }

        public DateTimeOffset TradeTime { get; set; }
        
        /// <summary>Last modification time (status change) of data</summary>
        public DateTimeOffset UpdatedAt { get; set; }

        public TradeState? State { get; set; }

        public string Currency { get; set; }

        public long EventSequenceNo { get; set; }
        
        public long RevisionNo { get; set; }
        
        /// <summary>A medium length display name for the contract.</summary>
        public string MediumDisplayName { get; set; }
        
        public bool? CompanyTrade { get; set; }
    }
}