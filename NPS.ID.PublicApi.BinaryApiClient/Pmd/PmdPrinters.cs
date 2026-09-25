using System.Text;
using NPS.ID.PublicApi.BinaryApiClient.Cli;
using NPS.Intraday.Edge.PMD.V2;

namespace NPS.ID.PublicApi.BinaryApiClient.Pmd;

/// <summary>Console printers for PMD server frames — one aligned line per row.</summary>
public static class PmdPrinters
{
    public static void PrintLocalView(LocalViewPayload payload)
    {
        var kind = payload.IsSnapshot ? "SNAPSHOT" : "DELTA   ";
        var sb = new StringBuilder();

        foreach (var lv in payload.LocalViews)
        {
            sb.AppendLine($"[LV {kind}] area={payload.DeliveryAreaId,-6} contract={lv.ContractId,-12} market={lv.MarketType,-20} rev={lv.RevisionNo}");

            if (lv.BuyOrders.Count > 0)
            {
                sb.AppendLine($"  Buy  ({lv.BuyOrders.Count}):");
                foreach (var o in lv.BuyOrders) AppendOrder(sb, o);
            }

            if (lv.SellOrders.Count > 0)
            {
                sb.AppendLine($"  Sell ({lv.SellOrders.Count}):");
                foreach (var o in lv.SellOrders) AppendOrder(sb, o);
            }
        }

        ConsoleIO.Write(sb.ToString().TrimEnd());
    }

    private static void AppendOrder(StringBuilder sb, Order o)
    {
        var status = o.Quantity == 0 ? "DELETED      " : $"qty={o.Quantity,-9}";
        sb.AppendLine($"    order={o.OrderId,-14} price={o.Price,-10} {status} ");
    }

    public static void PrintDeliveryArea(DeliveryAreaPayload payload)
    {
        var kind = payload.IsSnapshot ? "SNAPSHOT" : "DELTA   ";
        var sb = new StringBuilder();

        foreach (var da in payload.DeliveryAreas)
        {
            var products = string.Join(", ", da.ProductTypes);
            sb.AppendLine(
                $"[DA {kind}] id={da.DeliveryAreaId,-4} eic={da.EicCode,-16} area={da.AreaCode,-6} " +
                $"currency={da.CurrencyCode,-4} tz={da.TimeZone,-20} country={da.CountryIsoCode,-3} products=[{products}]");
        }

        ConsoleIO.Write(sb.ToString().TrimEnd());
    }

    public static void PrintContract(ContractPayload payload)
    {
        var kind = payload.IsSnapshot ? "SNAPSHOT" : "DELTA   ";
        var sb = new StringBuilder();

        foreach (var c in payload.Contracts)
        {
            var deliveryStart = Format.Time(c.DeliveryStartMs);
            var deliveryEnd   = Format.Time(c.DeliveryEndMs);
            sb.AppendLine(
                $"[CTR {kind}] id={c.ContractId,-12} name={c.ContractName,-20} market={c.MarketType,-12} " +
                $"product={c.ProductType,-24} delivery={deliveryStart} - {deliveryEnd}");

            foreach (var state in c.DeliveryAreaStates)
            {
                sb.AppendLine(
                    $"    area={state.DeliveryAreaId,-4} state={state.State,-20} " +
                    $"open={Format.Time(state.OpenAtMs)} close={Format.Time(state.ClosedAtMs)}");
            }
        }

        ConsoleIO.Write(sb.ToString().TrimEnd());
    }

    public static void PrintTicker(TickerPayload payload)
    {
        var kind = payload.IsSnapshot ? "SNAPSHOT" : "DELTA   ";
        var sb = new StringBuilder();

        foreach (var t in payload.Trades)
        {
            var tradeTime = Format.Time(t.TradeTimeMs);
            sb.AppendLine(
                $"[TKR {kind}] id={t.TradeId,-12} market={t.MarketType,-12} state={t.State,-24} " +
                $"rev={t.RevisionNo} time={tradeTime}");

            foreach (var leg in t.Legs)
            {
                sb.AppendLine(
                    $"    contract={leg.ContractId,-12} side={leg.Side,-6} price={leg.UnitPrice,-10} " +
                    $"qty={leg.Quantity,-8} area={leg.DeliveryAreaId}");
            }
        }

        ConsoleIO.Write(sb.ToString().TrimEnd());
    }

    public static void PrintH2HCapacity(H2HCapacityPayload payload)
    {
        var kind = payload.IsSnapshot ? "SNAPSHOT" : "DELTA   ";
        var sb = new StringBuilder();

        foreach (var c in payload.H2HCapacities)
        {
            var from = Format.Time(c.DeliveryStartMs);
            var to   = Format.Time(c.DeliveryEndMs);
            var pub  = Format.Time(c.PublishedAtMs);

            sb.AppendLine(
                $"[H2H {kind}] area={payload.DeliveryAreaId,-4} {c.DeliveryAreaFrom}->{c.DeliveryAreaTo}  " +
                $"delivery={from} - {to}  in={c.InCapacity} out={c.OutCapacity}  " +
                $"published={pub} ");
        }

        ConsoleIO.Write(sb.ToString().TrimEnd());
    }

    public static void PrintPublicStatistic(PublicStatisticPayload payload)
    {
        var kind = payload.IsSnapshot ? "SNAPSHOT" : "DELTA   ";
        var sb = new StringBuilder();

        foreach (var ps in payload.PublicStatistics)
        {
            sb.AppendLine(
                $"[PS {kind}] area={ps.DeliveryAreaId,-4} market={ps.MarketType,-12} contract={ps.ContractId,-12} " +
                $"last={ps.LastPrice?.ToString() ?? "-"} qty={ps.LastQuantity?.ToString() ?? "-"} " +
                $"high={ps.HighestPrice?.ToString() ?? "-"} low={ps.LowestPrice?.ToString() ?? "-"} " +
                $"vwap={ps.Vwap?.ToString() ?? "-"} turnover={ps.Turnover?.ToString() ?? "-"} " +
                $"tendency={ps.Tendency}");
        }

        ConsoleIO.Write(sb.ToString().TrimEnd());
    }

    public static void PrintAtcCapacity(AtcCapacityPayload payload)
    {
        var kind = payload.IsSnapshot ? "SNAPSHOT" : "DELTA   ";
        var sb = new StringBuilder();

        foreach (var c in payload.AtcCapacities)
        {
            var from = Format.Time(c.DeliveryStartMs);
            var to   = Format.Time(c.DeliveryEndMs);
            var upd  = Format.Time(c.UpdatedAtMs);

            sb.AppendLine(
                $"[ATC {kind}] area={payload.DeliveryAreaId,-4} {c.FromDeliveryAreaId}->{c.ToDeliveryAreaId}  " +
                $"delivery={from} - {to}  capacity={c.Capacity}  updated={upd}");
        }

        ConsoleIO.Write(sb.ToString().TrimEnd());
    }
}
