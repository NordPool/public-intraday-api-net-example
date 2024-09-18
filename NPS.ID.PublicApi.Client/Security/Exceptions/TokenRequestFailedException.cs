namespace NPS.ID.PublicApi.Client.Security.Exceptions;

public class TokenRequestFailedException(string message, Exception innerException) : Exception(message, innerException);