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
    public class OrderExecutionEntry
    {
        public long EventSequenceNo { get; set; }
        public string MarketId { get; set; }
        public string TenantId { get; set; }
        public string UserId { get; set; }
        public string OrderId { get; set; }
        public long RevisionNo { get; set; }
        public string PreviousOrderId { get; set; }
        public string OriginalOrderId { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public string ClientOrderId { get; set; }
        public string PortfolioId { get; set; }
        public List<string> ContractIds { get; set; }
        public long DeliveryAreaId { get; set; }
        public OrderSideEnum Side { get; set; }
        public OrderTypeEnum OrderType { get; set; }
        public long UnitPrice { get; set; }
        public long Quantity { get; set; }
        public TimeInForceEnum TimeInForce { get; set; }
        public DateTimeOffset SxpireTime { get; set; }
        public string Text { get; set; }
        public OrderStateEnum State { get; set; }
        public OrderActionEnum Action { get; set; }
        public long ClipSize { get; set; }
        public long ClipPriceChange { get; set; }
        public long RemainingQuantity { get; set; }
        public List<Error> Errors { get; set; }
    }

}
