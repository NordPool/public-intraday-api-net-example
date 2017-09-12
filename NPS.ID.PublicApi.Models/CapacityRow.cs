using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPS.ID.PublicApi.Models
{
    [Description("TODO: Description")]
    public class CapacityRow : BaseRow
    {
        public int EventSequenceNo { get; set; }
        public bool Internal { get; set; }
        public DateTimeOffset PublicationTime { get; set; }
        public long DeliveryAreaFrom { get; set; }
        public long DeliveryAreaTo { get; set; }
        public long DeliveryAreaStart { get; set; }
        public long DeliveryAreaEnd { get; set; }
        public int InCapacity { get; set; }
        public int OutCapacity { get; set; }
    }
}
