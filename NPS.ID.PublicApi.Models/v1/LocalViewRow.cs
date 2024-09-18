/*
 *  Copyright 2017 Nord Pool.
 *  This library is intended to aid integration with Nord Pool�s Intraday API and comes without any warranty. Users of this library are responsible for separately testing and ensuring that it works according to their own standards.
 *  Please send feedback to idapi@nordpoolgroup.com.
 */

using System;
using System.Collections.Generic;

namespace Nordpool.ID.PublicApi.v1
{
	public class LocalViewRow
	{
		/// <summary>Last modification time (status change) of data</summary>
		public DateTimeOffset UpdatedAt { get; set; }
		
		/// <summary>Basic data for BUY orders</summary>
		public List<Order.Order> BuyOrders { get; set; }

		/// <summary>Basic data for SELL orders</summary>
		public List<Order.Order> SellOrders { get; set; }

		/// <summary>ID of the contract that this order belongs to</summary>
		public string ContractId { get; set; }

		public long DeliveryAreaId { get; set; }
	}
}
