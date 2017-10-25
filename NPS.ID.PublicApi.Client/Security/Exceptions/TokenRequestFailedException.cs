/*
 *  Copyright 2017 Nord Pool.
 *  This library is intended to aid integration with Nord Pool’s Intraday API and comes without any warranty. Users of this library are responsible for separately testing and ensuring that it works according to their own standards.
 *  Please send feedback to idapi@nordpoolgroup.com.
 */

using System;
using System.Runtime.Serialization;

namespace NPS.ID.PublicApi.Client.Security.Exceptions
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