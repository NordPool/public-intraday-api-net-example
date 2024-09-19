/*
 *  Copyright 2017 Nord Pool.
 *  This library is intended to aid integration with Nord Pool�s Intraday API and comes without any warranty. Users of this library are responsible for separately testing and ensuring that it works according to their own standards.
 *  Please send feedback to idapi@nordpoolgroup.com.
 */

namespace Nordpool.ID.PublicApi.v1.Trade.Leg
{
	public class PrivateTradeLeg  : BaseTradeLeg
	{
		public string PortfolioId { get; set; }

		public string RefOrderId { get; set; }

		public string UserId { get; set; }

		public long DeliveryStart { get; set; }

		public long DeliveryEnd { get; set; }

		public Order.OrderState? OrderState { get; set; }

		public Order.OrderType? OrderType { get; set; }

		public string Text { get; set; }

		public Order.OrderAction? OrderAction { get; set; }

		public Order.TimeInForce? TimeInForce { get; set; }

		public string ClientOrderId { get; set; }

	}
}
