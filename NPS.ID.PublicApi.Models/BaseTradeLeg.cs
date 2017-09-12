using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPS.ID.PublicApi.Models
{
    [Description("TODO: Description")]
    public abstract class BaseTradeLeg
    {
        public string ContractId { get; set; }
        public OrderSideEnum Side { get; set; }
        public long UnitPrice { get; set; }
        public long Quantity { get; set; }
        public long DeliveryAreaId { get; set; }
        public bool Aggressor { get; set; }
    }
}
