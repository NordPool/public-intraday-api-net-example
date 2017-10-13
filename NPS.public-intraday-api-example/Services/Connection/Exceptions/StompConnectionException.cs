using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Stomp.Net;

namespace NPS.public_intraday_api_example.Services.Connection.Exceptions
{
    public class StompConnectionException : Exception
    {
        public StompConnectionException()
        {
        }

        public StompConnectionException(string message) : base(message)
        {
        }

        public StompConnectionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected StompConnectionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}