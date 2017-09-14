//----------------------
// <auto-generated>
//     Generated using the NJsonSchema v9.5.4.0 (http://NJsonSchema.org)
// </auto-generated>
//----------------------

namespace NPS.ID.PublicApi.Models.Generated
{
    #pragma warning disable // Disable all warnings

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "9.5.4.0")]
    public partial class DeliveryAreaRow 
    {
        /// <summary>Delivery Area ID</summary>
        [Newtonsoft.Json.JsonProperty("deliveryAreaId", Required = Newtonsoft.Json.Required.Always)]
        public int DeliveryAreaId { get; set; }
    
        /// <summary>Delivery area EIC (Energy Identification Code)</summary>
        [Newtonsoft.Json.JsonProperty("eicCode", Required = Newtonsoft.Json.Required.AllowNull)]
        public string EicCode { get; set; }
    
        /// <summary>Currency code used in the area</summary>
        [Newtonsoft.Json.JsonProperty("currentyCode", Required = Newtonsoft.Json.Required.AllowNull)]
        public string CurrentyCode { get; set; }
    
        /// <summary>Delivery area code</summary>
        [Newtonsoft.Json.JsonProperty("areaCode", Required = Newtonsoft.Json.Required.AllowNull)]
        public string AreaCode { get; set; }
    
        /// <summary>Time zone used in the area</summary>
        [Newtonsoft.Json.JsonProperty("timeZone", Required = Newtonsoft.Json.Required.AllowNull)]
        public string TimeZone { get; set; }
    
        /// <summary>Country ISO code</summary>
        [Newtonsoft.Json.JsonProperty("countryIsoCode", Required = Newtonsoft.Json.Required.AllowNull)]
        public string CountryIsoCode { get; set; }
    
        /// <summary>Product types available in the area</summary>
        [Newtonsoft.Json.JsonProperty("productTypes", Required = Newtonsoft.Json.Required.AllowNull)]
        public System.Collections.Generic.List<string> ProductTypes { get; set; }
    
        /// <summary>If false: update information with the contents received, If true: delete entity indicated in the message</summary>
        [Newtonsoft.Json.JsonProperty("deleted", Required = Newtonsoft.Json.Required.Always)]
        public bool Deleted { get; set; }
    
        /// <summary>Last modification time (status change) of data</summary>
        [Newtonsoft.Json.JsonProperty("updatedAt", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        public System.DateTimeOffset UpdatedAt { get; set; }
    
        public string ToJson() 
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
        
        public static DeliveryAreaRow FromJson(string data)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<DeliveryAreaRow>(data);
        }
    }
}