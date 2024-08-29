namespace NPS.ID.PublicApi.DotNet.Client.Security;

public interface ISsoService
{
    /// <summary>
    /// Gets the auth token with the parameters given in the constructor
    /// </summary>
    /// <returns>the auth token as string</returns>
    Task<string> GetAuthTokenAsync(string userName, string password, CancellationToken cancellationToken = default);
}