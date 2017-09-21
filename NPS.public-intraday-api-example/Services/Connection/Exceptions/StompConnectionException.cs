/*
 *  Copyright 2017 Nord Pool.
 *  This library is intended to aid integration with Nord Pool’s Intraday API and comes without any warranty. Users of this library are responsible for separately testing and ensuring that it works according to their own standards.
 *  Please send feedback to idapi@nordpoolgroup.com.
 */

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