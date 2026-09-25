using System.Text;
using NPS.ID.PublicApi.BinaryApiClient.Cli;
using NPS.Intraday.Edge.Trading.V2;

namespace NPS.ID.PublicApi.BinaryApiClient.Trading;

/// <summary>Console printers for Trading API server frames — one aligned line per row.</summary>
public static class TradingPrinters
{
    public static void PrintError(string? requestId, ErrorPayload error)
    {
        ConsoleIO.WriteError(
            $"[TRD] [ERROR      ] code={error.Code} fatal={error.IsFatal} request_id={Format.Opt(requestId)} " +
            $"message={error.Message}");
    }

    public static void PrintTradingAck(string? requestId, TradingAckPayload ack)
    {
        var sb = new StringBuilder();
        var rejected = ack.RequestErrors.Count > 0 || ack.Acks.Any(a => a.State == OrderAckState.Rejected);

        sb.AppendLine($"[ACK] request_id={Format.Opt(requestId)} acks={ack.Acks.Count} request_errors={ack.RequestErrors.Count}");

        foreach (var e in ack.RequestErrors)
            sb.AppendLine($"    request error: {e.Code}: {e.Message}");

        foreach (var a in ack.Acks)
        {
            var id = a.HasOrderId
                ? $"order_id={a.OrderId} market={a.MarketType}"
                : $"client_order_id={a.ClientOrderId}";
            sb.AppendLine($"    {a.State,-26} {id}");
            foreach (var e in a.Errors)
                sb.AppendLine($"        {e.Code}: {e.Message}");
        }

        var text = sb.ToString().TrimEnd();
        if (rejected) ConsoleIO.WriteError(text); else ConsoleIO.Write(text);
    }

    public static void PrintOrderExecutionReport(string? requestId, OrderExecutionReportPayload payload)
    {
        var kind = payload.IsSnapshot ? "SNAPSHOT" : "DELTA   ";
        var sb = new StringBuilder();

        if (payload.IsSnapshot)
            sb.AppendLine($"[OER {kind}] rows={payload.OrderExecutionReports.Count} request_id={Format.Opt(requestId)}");

        foreach (var o in payload.OrderExecutionReports)
        {
            sb.AppendLine(
                $"[OER {kind}] market={o.MarketType,-18} order_id={o.OrderId,-12} rev={o.RevisionNo,-3} " +
                $"client_order_id={o.ClientOrderId,-38} portfolio={o.PortfolioId,-10} contract={o.ContractId,-12} area={o.DeliveryAreaId,-4} " +
                $"side={o.Side,-16} type={o.OrderType,-18} price={o.UnitPrice,-8} qty={o.Quantity,-8} " +
                $"rem={(o.HasRemainingQuantity ? o.RemainingQuantity.ToString() : "-"),-8} tif={o.TimeInForce,-28} " +
                $"state={o.State,-18} action={o.Action,-32} issuer={o.ActionIssuer,-28} " +
                $"updated={Format.Time(o.UpdatedAtMs)} text={Format.Opt(o.HasText ? o.Text : null)}");
        }

        ConsoleIO.Write(sb.ToString().TrimEnd());
    }

    public static void PrintPrivateTrade(string? requestId, PrivateTradePayload payload)
    {
        var kind = payload.IsSnapshot ? "SNAPSHOT" : "DELTA   ";
        var sb = new StringBuilder();

        if (payload.IsSnapshot)
            sb.AppendLine($"[PT {kind}] rows={payload.PrivateTrades.Count} request_id={Format.Opt(requestId)}");

        foreach (var t in payload.PrivateTrades)
        {
            sb.AppendLine(
                $"[PT {kind}] market={t.MarketType,-18} trade_id={t.TradeId,-12} rev={t.RevisionNo,-3} state={t.State,-24} " +
                $"phase={t.ContractPhase,-26} company_trade={t.CompanyTrade,-5} time={Format.Time(t.TradeTimeMs)}");

            foreach (var leg in t.Legs)
            {
                sb.AppendLine(
                    $"    contract={leg.ContractId,-12} ref_order_id={leg.RefOrderId,-12} client_order_id={leg.ClientOrderId,-38} " +
                    $"portfolio={leg.PortfolioId,-10} user={leg.UserId,-12} side={leg.Side,-16} type={leg.OrderType,-18} " +
                    $"price={leg.UnitPrice,-8} qty={leg.Quantity,-8} area={leg.DeliveryAreaId,-4} " +
                    $"aggressor={(leg.HasAggressor ? leg.Aggressor.ToString() : "-"),-5} " +
                    $"delivery={Format.Time(leg.DeliveryStartMs)} - {Format.Time(leg.DeliveryEndMs)}");
            }
        }

        ConsoleIO.Write(sb.ToString().TrimEnd());
    }

    public static void PrintThrottlingLimit(ThrottlingLimitPayload payload)
    {
        var sb = new StringBuilder();

        foreach (var l in payload.OrderLimits)
        {
            sb.AppendLine(
                $"[TL] market={l.MarketType,-18} orders_10s={l.Orders10S}/{l.Orders10SLimit}  orders_1h={l.Orders1H}/{l.Orders1HLimit}");
        }

        ConsoleIO.Write(sb.ToString().TrimEnd());
    }

    public static void PrintConfiguration(string? requestId, ConfigurationPayload payload)
    {
        var sb = new StringBuilder();

        foreach (var row in payload.Configurations)
        {
            sb.AppendLine(
                $"[CFG] portfolios={row.Portfolios.Count} company_users={row.CompanyUsers.Count} " +
                $"markets={row.MarketConfiguration.Count} request_id={Format.Opt(requestId)}");

            foreach (var p in row.Portfolios)
            {
                var areas = string.Join(",", p.Areas.Select(a => a.AreaId));
                var markets = string.Join(",", p.Markets.Select(m => m.MarketId));
                sb.AppendLine(
                    $"    portfolio={p.Id,-10} name={p.Name,-30} short={p.ShortName,-10} company={p.CompanyId,-8} " +
                    $"permission={p.Permission,-28} state={p.State,-22} deleted={p.Deleted,-5} currency={p.Currency,-4} " +
                    $"valid={Format.Time(p.ValidFromMs)} - {Format.Time(p.ValidToMs)} areas=[{areas}] markets=[{markets}]");
            }

            foreach (var m in row.MarketConfiguration)
                sb.AppendLine($"    market={m.MarketType,-18} tick_size={m.TickSize} lot_size={m.LotSize}");

            foreach (var (userId, name) in row.CompanyUsers)
                sb.AppendLine($"    user={userId,-12} name={name}");
        }

        ConsoleIO.Write(sb.ToString().TrimEnd());
    }
}
