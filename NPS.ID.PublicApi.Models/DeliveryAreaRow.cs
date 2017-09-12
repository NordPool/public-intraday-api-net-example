using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPS.ID.PublicApi.Models
{
    [Description("TODO: Description")]
    public class DeliveryAreaRow : BaseRow
    {
        public long DeliveryAreaId { get; set; }
        public string EicCode { get; set; }
        public string CurrentyCode { get; set; }
        public string AreaCode { get; set; }
        public string TimeZone { get; set; }
        public string CountryIsoCode { get; set; }
        public List<string> ProductTypes { get; set; }
        public bool Deleted { get; set; }
    }
}
