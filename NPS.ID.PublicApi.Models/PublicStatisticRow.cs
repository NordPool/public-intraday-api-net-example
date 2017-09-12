using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPS.ID.PublicApi.Models
{
    [Description("TODO: Description")]
    public class PublicStatisticRow
    {
        public long LastPrice { get; set; }
        public long LastQuantity { get; set; }
        public DateTimeOffset LastTradeTime { get; set; }
        public long HighestPrice { get; set; }
        public long LowestPrice { get; set; }
        public long Vwap { get; set; }
        public long Turnover { get; set; }
        public long DayAheadPrice { get; set; }
        public TendencyEnum Tendency { get; set; }
    }
}
