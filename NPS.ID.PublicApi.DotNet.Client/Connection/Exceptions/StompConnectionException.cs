namespace NPS.ID.PublicApi.DotNet.Client.Connection.Exceptions;

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
}