//----------------------
// <auto-generated>
//     Generated using the NJsonSchema v9.5.4.0 (http://NJsonSchema.org)
// </auto-generated>
//----------------------

namespace NPS.ID.PublicApi.Models.Generated
{
    #pragma warning disable // Disable all warnings

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "9.5.4.0")]
    public partial class PrivateTradeLeg 
    {
        [Newtonsoft.Json.JsonProperty("portfolioId", Required = Newtonsoft.Json.Required.AllowNull)]
        public string PortfolioId { get; set; }
    
        [Newtonsoft.Json.JsonProperty("refOrderId", Required = Newtonsoft.Json.Required.AllowNull)]
        public string RefOrderId { get; set; }
    
        [Newtonsoft.Json.JsonProperty("userId", Required = Newtonsoft.Json.Required.AllowNull)]
        public string UserId { get; set; }
    
        [Newtonsoft.Json.JsonProperty("deliveryStart", Required = Newtonsoft.Json.Required.Always)]
        public int DeliveryStart { get; set; }
    
        [Newtonsoft.Json.JsonProperty("deliveryEnd", Required = Newtonsoft.Json.Required.Always)]
        public int DeliveryEnd { get; set; }
    
        [Newtonsoft.Json.JsonProperty("orderState", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public PrivateTradeLegOrderState OrderState { get; set; }
    
        [Newtonsoft.Json.JsonProperty("orderType", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public PrivateTradeLegOrderType OrderType { get; set; }
    
        [Newtonsoft.Json.JsonProperty("text", Required = Newtonsoft.Json.Required.AllowNull)]
        public string Text { get; set; }
    
        [Newtonsoft.Json.JsonProperty("orderAction", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public PrivateTradeLegOrderAction OrderAction { get; set; }
    
        [Newtonsoft.Json.JsonProperty("timeInForce", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public PrivateTradeLegTimeInForce TimeInForce { get; set; }
    
        /// <summary>Contract Id for the order</summary>
        [Newtonsoft.Json.JsonProperty("contractId", Required = Newtonsoft.Json.Required.AllowNull)]
        public string ContractId { get; set; }
    
        /// <summary>Buy or Sell</summary>
        [Newtonsoft.Json.JsonProperty("side", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public PrivateTradeLegSide Side { get; set; }
    
        /// <summary>Price</summary>
        [Newtonsoft.Json.JsonProperty("unitPrice", Required = Newtonsoft.Json.Required.Always)]
        public int UnitPrice { get; set; }
    
        /// <summary>Quantity</summary>
        [Newtonsoft.Json.JsonProperty("quantity", Required = Newtonsoft.Json.Required.Always)]
        public int Quantity { get; set; }
    
        /// <summary>Delivery area Id of the order.</summary>
        [Newtonsoft.Json.JsonProperty("deliveryAreaId", Required = Newtonsoft.Json.Required.Always)]
        public int DeliveryAreaId { get; set; }
    
        /// <summary>true if leg is the aggressor, null if the information is unavailable (for XBID trades)</summary>
        [Newtonsoft.Json.JsonProperty("aggressor", Required = Newtonsoft.Json.Required.Always)]
        public bool Aggressor { get; set; }
    
        public string ToJson() 
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
        
        public static PrivateTradeLeg FromJson(string data)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<PrivateTradeLeg>(data);
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "9.5.4.0")]
    public partial class PrivateTradeRow 
    {
        /// <summary>Cancellation fee for this trade</summary>
        [Newtonsoft.Json.JsonProperty("cancellationFee", Required = Newtonsoft.Json.Required.Always)]
        public int CancellationFee { get; set; }
    
        /// <summary>There is a deadline for trade cancellation, in accordance with market rules</summary>
        [Newtonsoft.Json.JsonProperty("cancellationDeadLine", Required = Newtonsoft.Json.Required.AllowNull)]
        public string CancellationDeadLine { get; set; }
    
        [Newtonsoft.Json.JsonProperty("revisionNo", Required = Newtonsoft.Json.Required.Always)]
        public int RevisionNo { get; set; }
    
        /// <summary>The timestamp at which this trade was updated (in case of trade cancellation).</summary>
        [Newtonsoft.Json.JsonProperty("updatedAt", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        public System.DateTimeOffset UpdatedAt { get; set; }
    
        /// <summary>Trade ID</summary>
        [Newtonsoft.Json.JsonProperty("tradeId", Required = Newtonsoft.Json.Required.AllowNull)]
        public string TradeId { get; set; }
    
        /// <summary>The timestamp when the trade was created.</summary>
        [Newtonsoft.Json.JsonProperty("tradeTime", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        public System.DateTimeOffset TradeTime { get; set; }
    
        /// <summary>COMPLETED - the trade is completed, CANCELLED - the trade is cancelled.</summary>
        [Newtonsoft.Json.JsonProperty("state", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public PrivateTradeRowState State { get; set; }
    
        /// <summary>Basic data about orders participated in the trade</summary>
        [Newtonsoft.Json.JsonProperty("legs", Required = Newtonsoft.Json.Required.AllowNull)]
        public System.Collections.Generic.List<PrivateTradeLeg> Legs { get; set; }
    
        /// <summary>Currency code</summary>
        [Newtonsoft.Json.JsonProperty("currency", Required = Newtonsoft.Json.Required.AllowNull)]
        public string Currency { get; set; }
    
        /// <summary>Sequence number for tracking received ticker events.</summary>
        [Newtonsoft.Json.JsonProperty("eventSequenceNo", Required = Newtonsoft.Json.Required.Always)]
        public int EventSequenceNo { get; set; }
    
        /// <summary>A flag that indicates if this trade should no longer be visible on the market (old trade).</summary>
        [Newtonsoft.Json.JsonProperty("deleted", Required = Newtonsoft.Json.Required.Always)]
        public bool Deleted { get; set; }
    
        /// <summary>A medium length display name for the contract.</summary>
        [Newtonsoft.Json.JsonProperty("mediumDisplayName", Required = Newtonsoft.Json.Required.AllowNull)]
        public string MediumDisplayName { get; set; }
    
        public string ToJson() 
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
        
        public static PrivateTradeRow FromJson(string data)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<PrivateTradeRow>(data);
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "9.5.4.0")]
    public enum PrivateTradeLegOrderState
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
    
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "9.5.4.0")]
    public enum PrivateTradeLegOrderType
    {
        [System.Runtime.Serialization.EnumMember(Value = "LIMIT")]
        LIMIT = 0,
    
        [System.Runtime.Serialization.EnumMember(Value = "ICEBERG")]
        ICEBERG = 1,
    
        [System.Runtime.Serialization.EnumMember(Value = "USER_DEFINED_BLOCK")]
        USER_DEFINED_BLOCK = 2,
    
    }
    
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "9.5.4.0")]
    public enum PrivateTradeLegOrderAction
    {
        [System.Runtime.Serialization.EnumMember(Value = "USER_ADDED")]
        USER_ADDED = 0,
    
        [System.Runtime.Serialization.EnumMember(Value = "USER_HIBERNATED")]
        USER_HIBERNATED = 1,
    
        [System.Runtime.Serialization.EnumMember(Value = "USER_MODIFIED")]
        USER_MODIFIED = 2,
    
        [System.Runtime.Serialization.EnumMember(Value = "USER_DELETED")]
        USER_DELETED = 3,
    
        [System.Runtime.Serialization.EnumMember(Value = "SYSTEM_HIBERNATED")]
        SYSTEM_HIBERNATED = 4,
    
        [System.Runtime.Serialization.EnumMember(Value = "SYSTEM_MODIFIED")]
        SYSTEM_MODIFIED = 5,
    
        [System.Runtime.Serialization.EnumMember(Value = "SYSTEM_DELETED")]
        SYSTEM_DELETED = 6,
    
        [System.Runtime.Serialization.EnumMember(Value = "PARTIAL_EXECUTION")]
        PARTIAL_EXECUTION = 7,
    
        [System.Runtime.Serialization.EnumMember(Value = "FULL_EXECUTION")]
        FULL_EXECUTION = 8,
    
        [System.Runtime.Serialization.EnumMember(Value = "ICEBERG_SLICE_ADDED")]
        ICEBERG_SLICE_ADDED = 9,
    
    }
    
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "9.5.4.0")]
    public enum PrivateTradeLegTimeInForce
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
    public enum PrivateTradeLegSide
    {
        [System.Runtime.Serialization.EnumMember(Value = "BUY")]
        BUY = 0,
    
        [System.Runtime.Serialization.EnumMember(Value = "SELL")]
        SELL = 1,
    
    }
    
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "9.5.4.0")]
    public enum PrivateTradeRowState
    {
        [System.Runtime.Serialization.EnumMember(Value = "COMPLETED")]
        COMPLETED = 0,
    
        [System.Runtime.Serialization.EnumMember(Value = "DISPUTED")]
        DISPUTED = 1,
    
        [System.Runtime.Serialization.EnumMember(Value = "NOT_CANCELLED")]
        NOT_CANCELLED = 2,
    
        [System.Runtime.Serialization.EnumMember(Value = "CANCELLED")]
        CANCELLED = 3,
    
    }
}