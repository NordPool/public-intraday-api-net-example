using NodaTime;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPS.ID.PublicApi.Models
{
    [Description("TODO: Description")]
    public class OrderEntry
    {
        public string PortfolioId { get; set; }
        public string[] ContractIds { get; set; }
        public long DeliveryAreaId { get; set; }
        public OrderSideEnum Side { get; set; }
        public long? ClipSize { get; set; }
        public long? ClipPriceChange { get; set; }
        public OrderTypeEnum OrderType { get; set; }
        public long UnitPrice { get; set; }
        public long Quantity { get; set; }
        public TimeInForceEnum TimeInForce { get; set; }
        public ExecutionRestrictionEnum ExecutionRestriction { get; set; }
        public DateTimeOffset ExpireTime { get; set; }
        public OrderStateEnum State { get; set; }
        public string ClientOrderId { get; set; }
    }
}
