namespace NPS.ID.PublicApi.DotNet.Client.Connection.Subscriptions.Exceptions;

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
}