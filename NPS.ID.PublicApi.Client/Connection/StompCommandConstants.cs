using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPS.ID.PublicApi.Client.Connection
{
    public struct StompCommandConstants
    {
        public static readonly string Connect = "CONNECT";
        public static readonly string Disconnect = "DISCONNECT";
        public static readonly string Message = "MESSAGE";
        public static readonly string Send = "SEND";
        public static readonly string Connected = "CONNECTED";
        public static readonly string Error = "ERROR";

        public static readonly string Subscribe = "SUBSCRIBE";
        public static readonly string Unsubscribe = "UNSUBSCRIBE";
        public static readonly string Receipt = "RECEIPT";
    }
}