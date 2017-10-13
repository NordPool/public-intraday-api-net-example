using System;
using System.Runtime.Serialization;

namespace NPS.public_intraday_api_example.Services.Security.Exceptions
{
    public class TokenRequestFailedException : Exception
    {
        public TokenRequestFailedException()
        {
        }

        public TokenRequestFailedException(string message) : base(message)
        {
        }

        public TokenRequestFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TokenRequestFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}