//----------------------
// <auto-generated>
//     Generated using the NJsonSchema v9.5.4.0 (http://NJsonSchema.org)
// </auto-generated>
//----------------------

namespace NPS.ID.PublicApi.Models.Generated
{
    #pragma warning disable // Disable all warnings

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "9.5.4.0")]
    public partial class OrderEntry 
    {
        /// <summary>The portfolio id of the current order</summary>
        [Newtonsoft.Json.JsonProperty("portfolioId", Required = Newtonsoft.Json.Required.AllowNull)]
        public string PortfolioId { get; set; }
    
        /// <summary>The contract ids that the current order should be placed on. For limit orders, only one value is allowed, for custom block orders all the contracts that the block spans should be included</summary>
        [Newtonsoft.Json.JsonProperty("contractIds", Required = Newtonsoft.Json.Required.AllowNull)]
        public System.Collections.Generic.List<string> ContractIds { get; set; }
    
        /// <summary>The delivery area id of the current order.</summary>
        [Newtonsoft.Json.JsonProperty("deliveryAreaId", Required = Newtonsoft.Json.Required.Always)]
        public int DeliveryAreaId { get; set; }
    
        /// <summary>BUY/SELL</summary>
        [Newtonsoft.Json.JsonProperty("side", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public OrderEntrySide Side { get; set; }
    
        /// <summary>For iceberg orders only; the size of one clip</summary>
        [Newtonsoft.Json.JsonProperty("clipSize", Required = Newtonsoft.Json.Required.AllowNull)]
        public int? ClipSize { get; set; }
    
        /// <summary>For iceberg orders only; the price change after each clip is consumed</summary>
        [Newtonsoft.Json.JsonProperty("clipPriceChange", Required = Newtonsoft.Json.Required.AllowNull)]
        public int? ClipPriceChange { get; set; }
    
        /// <summary>LIMIT, ICEBERG, USER_DEFINED_BLOCK</summary>
        [Newtonsoft.Json.JsonProperty("orderType", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public OrderEntryOrderType OrderType { get; set; }
    
        /// <summary>Price in Euro cents or pence (based on currency for the area)</summary>
        [Newtonsoft.Json.JsonProperty("unitPrice", Required = Newtonsoft.Json.Required.Always)]
        public int UnitPrice { get; set; }
    
        /// <summary>Quantity in kW</summary>
        [Newtonsoft.Json.JsonProperty("quantity", Required = Newtonsoft.Json.Required.Always)]
        public int Quantity { get; set; }
    
        /// <summary>IOC, FOK, AON, NON, GTD, GFS</summary>
        [Newtonsoft.Json.JsonProperty("timeInForce", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public OrderEntryTimeInForce TimeInForce { get; set; }
    
        /// <summary>“AON” (All or None): The order must be filled completely or not at all. The order stays in the order book until it is executed or removed by the system or user. This execution restriction can be used only in combination with User Defined Block Orders. “NON”: No restrictions.</summary>
        [Newtonsoft.Json.JsonProperty("executionRestriction", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public OrderEntryExecutionRestriction ExecutionRestriction { get; set; }
    
        /// <summary>If timeInForce is set to GTD (Good Till Date), the expireTime will determine when the order expires</summary>
        [Newtonsoft.Json.JsonProperty("expireTime", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        public System.DateTimeOffset ExpireTime { get; set; }
    
        /// <summary>ACTI — Active, IACT — Closed, matched(will never be reopened), HIBE — Deactivated(can be reopened)</summary>
        [Newtonsoft.Json.JsonProperty("state", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public OrderEntryState State { get; set; }
    
        /// <summary>UUID for the order, provided by the client to track their own orders</summary>
        [Newtonsoft.Json.JsonProperty("clientOrderId", Required = Newtonsoft.Json.Required.AllowNull)]
        public string ClientOrderId { get; set; }
    
        public string ToJson() 
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
        
        public static OrderEntry FromJson(string data)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<OrderEntry>(data);
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "9.5.4.0")]
    public partial class OrderEntryRequest 
    {
        /// <summary>Unique identifier for this request, provided by the client to track their own requests</summary>
        [Newtonsoft.Json.JsonProperty("requestId", Required = Newtonsoft.Json.Required.AllowNull)]
        public string RequestId { get; set; }
    
        /// <summary>Should the message be completely rejected if only some of the entered orders cause errors.</summary>
        [Newtonsoft.Json.JsonProperty("rejectPartially", Required = Newtonsoft.Json.Required.Always)]
        public bool RejectPartially { get; set; }
    
        /// <summary>List of orders</summary>
        [Newtonsoft.Json.JsonProperty("orders", Required = Newtonsoft.Json.Required.AllowNull)]
        public System.Collections.Generic.List<OrderEntry> Orders { get; set; }
    
        public string ToJson() 
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
        
        public static OrderEntryRequest FromJson(string data)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<OrderEntryRequest>(data);
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "9.5.4.0")]
    public enum OrderEntrySide
    {
        [System.Runtime.Serialization.EnumMember(Value = "BUY")]
        BUY = 0,
    
        [System.Runtime.Serialization.EnumMember(Value = "SELL")]
        SELL = 1,
    
    }
    
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "9.5.4.0")]
    public enum OrderEntryOrderType
    {
        [System.Runtime.Serialization.EnumMember(Value = "LIMIT")]
        LIMIT = 0,
    
        [System.Runtime.Serialization.EnumMember(Value = "ICEBERG")]
        ICEBERG = 1,
    
        [System.Runtime.Serialization.EnumMember(Value = "USER_DEFINED_BLOCK")]
        USER_DEFINED_BLOCK = 2,
    
    }
    
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "9.5.4.0")]
    public enum OrderEntryTimeInForce
    {
        [System.Runtime.Serialization.EnumMember(Value = "IOC")]
        IOC = 0,
    
        [System.Runtime.Serialization.EnumMember(Value = "FOK")]
        FOK = 1,
    
        [System.Runtime.Serialization.EnumMember(Value = "NON")]
        NON = 2,
    
        [System.Runtime.Serialization.EnumMember(Value = "GTD")]
        GTD = 3,
    
        [System.Runtime.Serialization.EnumMember(Value = "GFS")]
        GFS = 4,
    
    }
    
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "9.5.4.0")]
    public enum OrderEntryExecutionRestriction
    {
        [System.Runtime.Serialization.EnumMember(Value = "AON")]
        AON = 0,
    
        [System.Runtime.Serialization.EnumMember(Value = "NON")]
        NON = 1,
    
    }
    
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "9.5.4.0")]
    public enum OrderEntryState
    {
        [System.Runtime.Serialization.EnumMember(Value = "PENDING")]
        PENDING = 0,
    
        [System.Runtime.Serialization.EnumMember(Value = "ACTI")]
        ACTI = 1,
    
        [System.Runtime.Serialization.EnumMember(Value = "HIBE")]
        HIBE = 2,
    
        [System.Runtime.Serialization.EnumMember(Value = "IACT")]
        IACT = 3,
    
        [System.Runtime.Serialization.EnumMember(Value = "REJECTED")]
        REJECTED = 4,
    
    }
}