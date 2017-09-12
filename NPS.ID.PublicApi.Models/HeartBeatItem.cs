using System.ComponentModel;

namespace NPS.ID.PublicApi.Models
{
    [Description("TODO: Description")]
    public class HeartBeatItem
    {
        public string Topic { get; set; }
        public long LastSequenceNumber { get; set; }
    }
}