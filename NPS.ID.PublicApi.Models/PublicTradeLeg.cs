using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPS.ID.PublicApi.Models
{
    [Description("TODO: Description")]
    public class PublicTradeLeg : BaseTradeLeg
    {
        public LegOwnershipEnum Ownership { get; set; }
    }
}
