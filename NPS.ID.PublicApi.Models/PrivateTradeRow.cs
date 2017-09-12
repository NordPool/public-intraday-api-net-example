using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPS.ID.PublicApi.Models
{
    [Description("TODO: Description")]
    public class PrivateTradeRow : BaseTradeRow<PrivateTradeLeg>
    {
        public long CancellationFee { get; set; }
        public string CancellationDeadLine { get; set; }
        public long RevisionNo { get; set; }
    }
}
