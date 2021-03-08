/*
 *  Copyright 2017 Nord Pool.
 *  This library is intended to aid integration with Nord Pool’s Intraday API and comes without any warranty. Users of this library are responsible for separately testing and ensuring that it works according to their own standards.
 *  Please send feedback to idapi@nordpoolgroup.com.
 */

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPS.ID.PublicApi.Client.WinFormsExample
{
    public class JsonHelper
    {

        public static string SerializeObjectPrettyPrinted<T>(T objectToSerialize)
        {
            JsonSerializerSettings serSettings = MakeSerializerSettings(true);
            return JsonConvert.SerializeObject(objectToSerialize, serSettings);
        }

        public static string SerializeObjectNotPrettyPrinted<T>(T objectToSerialize)
        {
            JsonSerializerSettings serSettings = MakeSerializerSettings(false);
            return JsonConvert.SerializeObject(objectToSerialize, serSettings);
        }

        private static JsonSerializerSettings MakeSerializerSettings(bool usePrettyPrint)
        {
            var serSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            };
            serSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

            if (usePrettyPrint)
            {
                serSettings.Formatting = Formatting.Indented;
            }

            serSettings.Converters.Add(new IsoDateTimeConverter
            {
                Culture = CultureInfo.InvariantCulture,
                DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ",
                DateTimeStyles = DateTimeStyles.AdjustToUniversal
            });

            return serSettings;
        }

        public static T DeserializeData<T>(string JsonMessage) where T : new()
        {
            try
            {
                var serSettings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    NullValueHandling = NullValueHandling.Ignore
                };
                
                T obj = JsonConvert.DeserializeObject<T>(JsonMessage, serSettings);
                return obj;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
