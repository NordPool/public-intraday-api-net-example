using NPS.ID.PublicApi.BinaryApiClient.Pmd;
using NPS.Intraday.Edge.Trading.V2;
using PmdMarketType = NPS.Intraday.Edge.PMD.V2.MarketType;
using PmdContract = NPS.Intraday.Edge.PMD.V2.Contract;
using PmdContractState = NPS.Intraday.Edge.PMD.V2.ContractState;

namespace NPS.ID.PublicApi.BinaryApiClient.Trading;

/// <summary>Raised when a console command cannot be turned into a valid request; the message is shown to the user.</summary>
public sealed class CommandException(string message) : Exception(message);

/// <summary>
/// Builds trading requests from cached market data / configuration / order state plus optional
/// <c>key=value</c> overrides typed on the console.
/// </summary>
public sealed class OrderBuilder(ContractCache contracts, ConfigurationCache configuration, OrderCache orders)
{
    // Defaults mirror the STOMP example: a resting SELL LIMIT order that is unlikely to match.
    private const long DefaultQuantity = 3000;
    private const long DefaultUnitPrice = 2500;
    private const int DefaultGtdExpiryMinutes = 360;
    private const string DefaultText = "Binary API example order";

    // -------------------------------------------------------------------------
    // Order entry
    // -------------------------------------------------------------------------

    /// <summary>
    /// Builds an <see cref="OrderEntry"/>. Without overrides a random tradable contract / portfolio /
    /// area is picked from the caches (requires <c>csub</c> and <c>cfgsub</c> to have delivered data).
    /// Supported keys: side, qty, price, contract, portfolio, area, market, tif, expire (minutes), state, text.
    /// </summary>
    public OrderEntry BuildOrderEntry(IReadOnlyDictionary<string, string> o, out string clientOrderId)
    {
        var (contractId, portfolioId, areaId, market) = ResolveContractPortfolioArea(o);

        var side = o.TryGetValue("side", out var s) ? ParseEnum<OrderSide>(s, "ORDER_SIDE") : OrderSide.Sell;
        var tif  = o.TryGetValue("tif", out var t) ? ParseEnum<OrderTimeInForce>(t, "ORDER_TIME_IN_FORCE")
                 : o.ContainsKey("expire") ? OrderTimeInForce.Gtd
                 : OrderTimeInForce.Gfs;
        var state = o.TryGetValue("state", out var st) ? ParseEnum<OrderEntryState>(st, "ORDER_ENTRY_STATE") : OrderEntryState.Acti;

        clientOrderId = Guid.NewGuid().ToString();

        var order = new Order
        {
            PortfolioId = portfolioId,
            ClientOrderId = clientOrderId,
            DeliveryAreaId = areaId,
            ContractIds = { contractId },
            Type = OrderType.Limit,
            Side = side,
            Quantity = ParseLong(o, "qty", DefaultQuantity),
            UnitPrice = ParseLong(o, "price", DefaultUnitPrice),
            State = state,
            TimeInForce = tif,
            Text = o.GetValueOrDefault("text", DefaultText),
        };

        if (tif == OrderTimeInForce.Gtd)
            order.ExpireTimeMs = ResolveExpiry(o);

        return new OrderEntry { MarketType = market, RejectPartially = false, Orders = { order } };
    }

    /// <summary>An entry that is missing almost every required field — demonstrates TradingAck validation errors.</summary>
    public OrderEntry BuildInvalidOrderEntry(out string clientOrderId)
    {
        clientOrderId = Guid.NewGuid().ToString();
        var portfolioId = configuration.GetTradablePortfolios().FirstOrDefault()?.Id ?? "INVALID";

        return new OrderEntry
        {
            MarketType = MarketType.Local,
            Orders = { new Order { PortfolioId = portfolioId, ClientOrderId = clientOrderId } }
        };
    }

    // -------------------------------------------------------------------------
    // Modification
    // -------------------------------------------------------------------------

    /// <summary>
    /// Builds a <c>modify</c> for an order known from the OER cache. Unchanged fields are restated from
    /// the cached report. Supported keys: qty, price, tif, expire (minutes), text.
    /// </summary>
    public OrderModification BuildModify(string idOrAlias, IReadOnlyDictionary<string, string> o)
    {
        var oer = ResolveOrder(idOrAlias);

        var tif = o.TryGetValue("tif", out var t) ? ParseEnum<OrderTimeInForce>(t, "ORDER_TIME_IN_FORCE")
                : o.ContainsKey("expire") ? OrderTimeInForce.Gtd
                : oer.TimeInForce;

        var payload = new ModifyOrder
        {
            ContractId = oer.ContractId,
            Type = oer.OrderType,
            Quantity = ParseLong(o, "qty", oer.Quantity),
            UnitPrice = ParseLong(o, "price", oer.UnitPrice),
            TimeInForce = tif,
        };

        if (oer.OrderType == OrderType.Iceberg)
        {
            if (oer.HasClipSize) payload.ClipSize = oer.ClipSize;
            if (oer.HasClipPriceChange) payload.ClipPriceChange = oer.ClipPriceChange;
        }

        if (tif == OrderTimeInForce.Gtd)
            payload.ExpireTimeMs = o.ContainsKey("expire") ? ResolveExpiry(o)
                                 : oer.HasExpireTimeMs ? oer.ExpireTimeMs
                                 : ResolveExpiry(o);

        if (o.TryGetValue("text", out var text)) payload.Text = text;
        else if (oer.HasText) payload.Text = oer.Text;

        return new OrderModification
        {
            MarketType = oer.MarketType,
            Modify = new Modify
            {
                Items =
                {
                    new ModifyItem
                    {
                        Ref = new OrderRef { OrderId = oer.OrderId, PortfolioId = oer.PortfolioId, Revision = oer.RevisionNo },
                        Payload = payload
                    }
                }
            }
        };
    }

    /// <summary>Builds a <c>deactivate</c> for an order known from the OER cache (revision omitted → server uses latest).</summary>
    public OrderModification BuildDeactivate(string idOrAlias)
    {
        var oer = ResolveOrder(idOrAlias);

        return new OrderModification
        {
            MarketType = oer.MarketType,
            Deactivate = new Deactivate
            {
                Refs = { new OrderRef { OrderId = oer.OrderId, PortfolioId = oer.PortfolioId } }
            }
        };
    }

    /// <summary>A modify addressing a non-existent order — demonstrates ITEM_NOT_FOUND on the TradingAck.</summary>
    public OrderModification BuildInvalidModify()
    {
        var portfolioId = configuration.GetTradablePortfolios().FirstOrDefault()?.Id ?? "INVALID";

        return new OrderModification
        {
            MarketType = MarketType.Local,
            Modify = new Modify
            {
                Items =
                {
                    new ModifyItem
                    {
                        Ref = new OrderRef { OrderId = 1, PortfolioId = portfolioId, Revision = 1 },
                        Payload = new ModifyOrder
                        {
                            ContractId = 1,
                            Type = OrderType.Limit,
                            Quantity = DefaultQuantity,
                            UnitPrice = DefaultUnitPrice,
                            TimeInForce = OrderTimeInForce.Gfs
                        }
                    }
                }
            }
        };
    }

    // -------------------------------------------------------------------------
    // Resolution helpers
    // -------------------------------------------------------------------------

    private OrderExecutionReport ResolveOrder(string idOrAlias)
    {
        if (orders.Count == 0)
            throw new CommandException("No orders in cache — subscribe with 'oersub' first so order state can be resolved.");

        return orders.Resolve(idOrAlias)
               ?? throw new CommandException($"Order '{idOrAlias}' not found in cache (use an order_id, client_order_id or 'last').");
    }

    private (long ContractId, string PortfolioId, int AreaId, MarketType Market) ResolveContractPortfolioArea(
        IReadOnlyDictionary<string, string> o)
    {
        PmdContract? contract = null;

        // 1. Contract — explicit or random tradable one from the PMD contract cache.
        long contractId;
        if (o.TryGetValue("contract", out var c))
        {
            contractId = long.TryParse(c, out var parsed) ? parsed : throw new CommandException($"Invalid contract id '{c}'.");
            contract = contracts.Get(contractId);
        }
        else
        {
            var tradable = contracts.GetTradable();
            if (tradable.Count == 0)
                throw new CommandException("No tradable contract in cache — subscribe with 'csub' first, or pass contract=<id> market=<local|xbid>.");
            contract = tradable[Random.Shared.Next(tradable.Count)];
            contractId = contract.ContractId;
        }

        // 2. Market — explicit, or derived from the contract.
        MarketType market;
        if (o.TryGetValue("market", out var m))
            market = ParseEnum<MarketType>(m, "MARKET_TYPE");
        else if (contract is not null)
            market = MapMarket(contract.MarketType);
        else
            throw new CommandException($"Contract {contractId} is not in the PMD cache — pass market=<local|xbid> explicitly.");

        var activeAreas = contract?.DeliveryAreaStates
            .Where(s => s.State == PmdContractState.Acti)
            .Select(s => s.DeliveryAreaId)
            .ToHashSet() ?? [];

        // 3. Portfolio — explicit, or one with WRITE access to an area where the contract is active.
        Portfolio? portfolio;
        string portfolioId;
        if (o.TryGetValue("portfolio", out var p))
        {
            portfolioId = p;
            portfolio = configuration.GetPortfolio(p);
        }
        else
        {
            var candidates = configuration.GetTradablePortfolios()
                .Where(pf => activeAreas.Count == 0 || pf.Areas.Any(a => activeAreas.Contains(a.AreaId)))
                .ToList();
            if (candidates.Count == 0)
                throw new CommandException(configuration.HasData
                    ? $"No writable portfolio covers an active delivery area of contract {contractId} — pass portfolio=<id> area=<id>."
                    : "No configuration in cache — subscribe with 'cfgsub' first, or pass portfolio=<id> area=<id>.");
            portfolio = candidates[Random.Shared.Next(candidates.Count)];
            portfolioId = portfolio.Id;
        }

        // 4. Area — explicit, or the first portfolio area where the contract is active.
        int areaId;
        if (o.TryGetValue("area", out var a))
        {
            areaId = int.TryParse(a, out var parsedArea) ? parsedArea : throw new CommandException($"Invalid area id '{a}'.");
        }
        else
        {
            var portfolioAreas = portfolio?.Areas.Select(x => x.AreaId).ToList() ?? [];
            areaId = portfolioAreas.FirstOrDefault(x => activeAreas.Contains(x), 0);
            if (areaId == 0) areaId = activeAreas.FirstOrDefault();
            if (areaId == 0)
                throw new CommandException($"Could not determine a delivery area for contract {contractId} / portfolio {portfolioId} — pass area=<id>.");
        }

        return (contractId, portfolioId, areaId, market);
    }

    private static MarketType MapMarket(PmdMarketType pmd) => pmd switch
    {
        PmdMarketType.Local => MarketType.Local,
        PmdMarketType.Xbid => MarketType.Xbid,
        _ => throw new CommandException("Contract has no market type in the PMD cache — pass market=<local|xbid>.")
    };

    /// <summary>GTD expiry: now + <c>expire</c> minutes (default 6h), rounded down to a 5-minute boundary as required by the API.</summary>
    private static long ResolveExpiry(IReadOnlyDictionary<string, string> o)
    {
        var minutes = o.TryGetValue("expire", out var e)
            ? int.TryParse(e, out var parsed) && parsed > 0 ? parsed : throw new CommandException($"Invalid expire minutes '{e}'.")
            : DefaultGtdExpiryMinutes;

        var target = DateTimeOffset.UtcNow.AddMinutes(minutes);
        var fiveMinutesMs = 5 * 60 * 1000L;
        var ms = target.ToUnixTimeMilliseconds();
        return ms - ms % fiveMinutesMs;
    }

    private static long ParseLong(IReadOnlyDictionary<string, string> o, string key, long fallback) =>
        o.TryGetValue(key, out var v)
            ? long.TryParse(v, out var parsed) ? parsed : throw new CommandException($"Invalid value for {key}: '{v}'.")
            : fallback;

    /// <summary>Parses a short enum token (e.g. "buy", "gtd", "xbid") against the proto enum names.</summary>
    private static TEnum ParseEnum<TEnum>(string token, string protoPrefix) where TEnum : struct, Enum
    {
        // Proto enums generate as PascalCase without the prefix, e.g. ORDER_SIDE_BUY → Buy.
        if (Enum.TryParse<TEnum>(token, ignoreCase: true, out var value) && !value.Equals(default(TEnum)))
            return value;

        var valid = string.Join("|", Enum.GetNames<TEnum>().Where(n => !n.EndsWith("Unspecified")).Select(n => n.ToLowerInvariant()));
        throw new CommandException($"Invalid {protoPrefix.ToLowerInvariant()} '{token}' — expected one of: {valid}.");
    }
}
