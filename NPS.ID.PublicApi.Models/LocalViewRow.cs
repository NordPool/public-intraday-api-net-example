using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPS.ID.PublicApi.Models
{
    [Description("TODO: Description")]
    public class LocalViewRow : BaseRow
    {
        public List<Order> BuyOrders { get; set; }
        public List<Order> SellOrders { get; set; }
        public string ContractId { get; set; }
        public long DeliveryAreaId { get; set; }
        public string OrderExecutionRestriction { get; set; }
    }
}
