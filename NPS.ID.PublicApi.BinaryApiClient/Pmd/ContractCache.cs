using System.Collections.Concurrent;
using NPS.Intraday.Edge.PMD.V2;

namespace NPS.ID.PublicApi.BinaryApiClient.Pmd;

/// <summary>
/// In-memory cache of the latest <see cref="Contract"/> rows received on the PMD contract stream.
/// Used by the trading commands to pick a valid contract / market type for example orders.
/// </summary>
public sealed class ContractCache
{
    private readonly ConcurrentDictionary<long, Contract> _contracts = new();

    public int Count => _contracts.Count;

    public void Apply(ContractPayload payload)
    {
        foreach (var c in payload.Contracts)
            _contracts[c.ContractId] = c;
    }

    public Contract? Get(long contractId) =>
        _contracts.GetValueOrDefault(contractId);

    /// <summary>All cached contracts that are ACTI in at least one delivery area and are not custom blocks.</summary>
    public IReadOnlyList<Contract> GetTradable() =>
        _contracts.Values
            .Where(c => c.ProductType != ProductType.CustomBlock
                        && c.DeliveryAreaStates.Any(s => s.State == ContractState.Acti))
            .ToList();
}
