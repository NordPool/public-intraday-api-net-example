using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPS.ID.PublicApi.Models
{
    public class Error
    {
        
        public ErrorCodeEnum ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }
}
