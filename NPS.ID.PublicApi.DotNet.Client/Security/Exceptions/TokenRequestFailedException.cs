namespace NPS.ID.PublicApi.DotNet.Client.Security.Exceptions;

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
}