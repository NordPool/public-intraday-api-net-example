using NPS.Intraday.Edge.Trading.V2;

namespace NPS.ID.PublicApi.BinaryApiClient.Trading;

/// <summary>
/// Latest <see cref="ConfigurationRow"/> for the authenticated user (replace semantics — every
/// configuration message carries the full current profile).
/// </summary>
public sealed class ConfigurationCache
{
    private volatile ConfigurationRow? _current;

    public bool HasData => _current is not null;

    public void Apply(ConfigurationPayload payload)
    {
        // The server sends one row; take the last in case of several.
        var row = payload.Configurations.LastOrDefault();
        if (row is not null) _current = row;
    }

    /// <summary>Portfolios the user can trade on (WRITE permission, ACTI, not deleted).</summary>
    public IReadOnlyList<Portfolio> GetTradablePortfolios() =>
        _current?.Portfolios
            .Where(p => p.Permission == PortfolioPermission.Write
                        && p.State == PortfolioState.Acti
                        && !p.Deleted)
            .ToList() ?? [];

    public Portfolio? GetPortfolio(string id) =>
        _current?.Portfolios.FirstOrDefault(p => p.Id == id);
}
