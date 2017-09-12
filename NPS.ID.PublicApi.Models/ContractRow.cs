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
    public class ContractRow : BaseRow
    {
        public string ContractId { get; set; }
        public string ContractName { get; set; }
        public ContractStateEnum State { get; set; }
        public long DurationSeconds { get; set; }
        public long ResolutionSeconds { get; set; }
        public bool Predefined { get; set; }
        public bool Deleted { get; set; }
        public DateTimeOffset DlvryStart { get; set; }
        public DateTimeOffset DlvryEnd { get; set; }
        public List<DeliveryAreaState> DlvryAreaState { get; set; }
        public string MarketId { get; set; }
        public string ShortDisplayName { get; set; }
        public string MediumDisplayName { get; set; }
        public ProductTypeEnum ProductType { get; set; }
        public string ProductId { get; set; }
    }
}
