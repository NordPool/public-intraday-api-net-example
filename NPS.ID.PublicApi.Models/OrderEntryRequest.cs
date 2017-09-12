using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPS.ID.PublicApi.Models
{
    /// <summary>
    /// NOTE: This class is NOT the official contract for OrderEntryRequest and one should ALWAYS check the required fields from the official documentation: https://developers.nordpoolgroup.com/docs/creating-and-modifying-orders
    /// </summary>
    [Description("TODO: Description")]
    public class OrderEntryRequest
    {
        public string RequestId { get; set; }
        public bool RejectPartially { get; set; }
        public List<OrderEntry> Orders { get; set; }


    }
}
