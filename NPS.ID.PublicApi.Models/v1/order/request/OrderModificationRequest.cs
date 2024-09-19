/*
 *  Copyright 2017 Nord Pool.
 *  This library is intended to aid integration with Nord Pool�s Intraday API and comes without any warranty. Users of this library are responsible for separately testing and ensuring that it works according to their own standards.
 *  Please send feedback to idapi@nordpoolgroup.com.
 */

using System.Collections.Generic;

namespace Nordpool.ID.PublicApi.v1.Order.Request
{
	public class OrderModificationRequest 
	{
		/// <summary>Unique identifier for this request, provided by the client to track their own requests</summary>
		public string RequestId { get; set; }

		public OrderModificationType? OrderModificationType { get; set; }

		public List<OrderModification> Orders { get; set; }
	}
}
