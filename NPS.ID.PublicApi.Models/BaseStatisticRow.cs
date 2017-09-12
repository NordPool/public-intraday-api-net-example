using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPS.ID.PublicApi.Models
{
    [Description("TODO: Description")]
    public class BaseStatisticRow
    {
        public long DeliveryAreaId { get; set; }
        public string ContractId { get; set; }
    }
}
