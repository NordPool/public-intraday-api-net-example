/*
 *  Copyright 2017 Nord Pool.
 *  This library is intended to aid integration with Nord Pool�s Intraday API and comes without any warranty. Users of this library are responsible for separately testing and ensuring that it works according to their own standards.
 *  Please send feedback to idapi@nordpoolgroup.com.
 */

using System;

namespace Nordpool.ID.PublicApi.v1.Statistic
{
	/// <summary>Trade history</summary>
	public class TradeHistory 
	{
		public long UnitPrice { get; set; }

		public long Quantity { get; set; }

		/// <summary>Time of the last trade</summary>
		public DateTimeOffset TradeTime { get; set; }
	}
}
