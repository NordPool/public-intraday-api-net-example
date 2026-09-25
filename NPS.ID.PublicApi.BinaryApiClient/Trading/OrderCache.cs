using System.Collections.Concurrent;
using NPS.Intraday.Edge.Trading.V2;

namespace NPS.ID.PublicApi.BinaryApiClient.Trading;

/// <summary>
/// Latest <see cref="OrderExecutionReport"/> per order, fed by the order-execution-report stream.
/// The OER stream is the authoritative source of an order's current revision, portfolio and
/// contract — the values needed to build <see cref="OrderRef"/>s for modify / deactivate requests.
/// </summary>
public sealed class OrderCache
{
    private readonly ConcurrentDictionary<(MarketType Market, long OrderId), OrderExecutionReport> _byOrderId = new();
    private readonly ConcurrentDictionary<string, (MarketType Market, long OrderId)> _byClientOrderId = new();

    /// <summary>client_order_id of the most recent order entered from this console session.</summary>
    public string? LastEnteredClientOrderId { get; set; }

    public int Count => _byOrderId.Count;

    public void Apply(OrderExecutionReportPayload payload)
    {
        foreach (var oer in payload.OrderExecutionReports)
        {
            var key = (oer.MarketType, oer.OrderId);

            // Keep only the newest event per order.
            _byOrderId.AddOrUpdate(key, oer,
                (_, existing) => oer.EventSequenceNo >= existing.EventSequenceNo ? oer : existing);

            if (!string.IsNullOrEmpty(oer.ClientOrderId))
                _byClientOrderId[oer.ClientOrderId] = key;
        }
    }

    /// <summary>
    /// Resolves an order by numeric order id, client order id, or the literal <c>last</c>.
    /// </summary>
    public OrderExecutionReport? Resolve(string idOrAlias)
    {
        if (idOrAlias.Equals("last", StringComparison.OrdinalIgnoreCase))
        {
            if (LastEnteredClientOrderId is null) return null;
            idOrAlias = LastEnteredClientOrderId;
        }

        if (long.TryParse(idOrAlias, out var orderId))
        {
            // Order ids are market-scoped; pick whichever market has the id (they don't collide in practice).
            var match = _byOrderId.Where(kv => kv.Key.OrderId == orderId).Select(kv => kv.Value).FirstOrDefault();
            if (match is not null) return match;
        }

        return _byClientOrderId.TryGetValue(idOrAlias, out var key)
            ? _byOrderId.GetValueOrDefault(key)
            : null;
    }
}
