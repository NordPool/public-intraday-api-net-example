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
    public class DeliveryAreaState
    {
        public long DlvryAreaId { get; set; }
        public ContractStateEnum State { get; set; }
        public DateTimeOffset OpenAt { get; set; }
        public DateTimeOffset ClosedAt { get; set; }
    }
}
