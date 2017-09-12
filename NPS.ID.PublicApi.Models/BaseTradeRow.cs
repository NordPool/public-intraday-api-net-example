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
    public class BaseTradeRow<LEG> where LEG : BaseTradeLeg
    {
        public DateTimeOffset UpdatedAt { get; set; }
        public string TradeId { get; set; }
        public DateTimeOffset TradeTime { get; set; }
        public TradeStateEnum State { get; set; }
        public List<LEG> Legs { get; set; }
        public string Currency { get; set; }
        public int EventSequenceNo { get; set; }
        public bool Deleted { get; set; }
        public string MediumDisplayName { get; set; }
    }
}
