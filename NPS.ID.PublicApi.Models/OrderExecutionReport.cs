using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace NPS.ID.PublicApi.Models
{

    [Description("TODO: Description")]
    public class OrderExecutionReport : BaseRow
    {
        
        public string RequestId { get; set; }
        public string MemberId { get; set; }
        [EnumDataType(typeof(ErrorTypeEnum))]
        public ErrorTypeEnum ErrorType { get; set; }
        public List<Error> Errors { get; set; }
        public List<OrderExecutionEntry> Orders { get; set; }
    }
   
}
