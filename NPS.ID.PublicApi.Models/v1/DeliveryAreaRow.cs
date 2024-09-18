/*
 *  Copyright 2017 Nord Pool.
 *  This library is intended to aid integration with Nord Pool�s Intraday API and comes without any warranty. Users of this library are responsible for separately testing and ensuring that it works according to their own standards.
 *  Please send feedback to idapi@nordpoolgroup.com.
 */

using System;
using System.Collections.Generic;

namespace Nordpool.ID.PublicApi.v1
{
	public class DeliveryAreaRow
	{
		public long DeliveryAreaId { get; set; }
		
		/// <summary>Last modification time (status change) of data</summary>
		public DateTimeOffset UpdatedAt { get; set; }

		/// <summary>Delivery area EIC (Energy Identification Code)</summary>
		public string EicCode { get; set; }

		/// <summary>Currency code used in the area</summary>
		public string CurrencyCode { get; set; }

		/// <summary>Delivery area code</summary>
		public string AreaCode { get; set; }

		/// <summary>Time zone used in the area</summary>
		public string TimeZone { get; set; }

		/// <summary>Country ISO code</summary>
		public string CountryIsoCode { get; set; }

		/// <summary>Product types available in the area</summary>
		public List<string> ProductTypes { get; set; }

		/// <summary>If false: update information with the contents received, If true: delete entity indicated in the message</summary>
		public bool Deleted { get; set; }
	}
}
