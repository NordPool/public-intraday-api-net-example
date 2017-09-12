using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NPS.ID.PublicApi.Client.Subscription
{
    public class SubscriptionFailedException : Exception
    {
        public SubscriptionFailedException()
        {
        }

        public SubscriptionFailedException(string message) : base(message)
        {
        }

        public SubscriptionFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SubscriptionFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}