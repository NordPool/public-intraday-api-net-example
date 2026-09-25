namespace NPS.ID.PublicApi.BinaryApiClient;

/// <summary>Small formatting helpers shared by the PMD and Trading printers.</summary>
public static class Format
{
    /// <summary>Formats Unix milliseconds as ISO-8601 UTC; "-" when zero/absent.</summary>
    public static string Time(long ms) =>
        ms == 0 ? "-" : DateTimeOffset.FromUnixTimeMilliseconds(ms).UtcDateTime.ToString("O");

    /// <summary>Formats a nullable value, "-" when absent.</summary>
    public static string Opt<T>(T? value) where T : struct =>
        value.HasValue ? value.Value.ToString() ?? "-" : "-";

    /// <summary>Formats a string, "-" when null/empty.</summary>
    public static string Opt(string? value) =>
        string.IsNullOrEmpty(value) ? "-" : value;
}
