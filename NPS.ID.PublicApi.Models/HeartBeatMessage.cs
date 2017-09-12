using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPS.ID.PublicApi.Models
{
    [Description("TODO: Description")]
    public class HeartbeatMessage
    {
        public long Timestamp { get; set; }
        public List<HeartBeatItem> HeartBeats { get; set; }

    }
}
