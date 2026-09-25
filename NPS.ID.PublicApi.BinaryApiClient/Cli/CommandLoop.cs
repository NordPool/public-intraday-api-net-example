using NPS.ID.PublicApi.BinaryApiClient.Pmd;
using NPS.ID.PublicApi.BinaryApiClient.Trading;
using CapacityFlow = NPS.Intraday.Edge.PMD.V2.CapacityFlow;

namespace NPS.ID.PublicApi.BinaryApiClient.Cli;

/// <summary>
/// Interactive console loop shared by the PMD and Trading connections.
/// Commands are dispatched to the respective client; nothing is scripted.
/// </summary>
public sealed class CommandLoop(PmdClient pmd, TradingClient trading)
{
    private readonly OrderBuilder _orders = new(pmd.Contracts, trading.Configuration, trading.Orders);

    public async Task RunAsync(CancellationToken ct)
    {
        PrintHelp();

        while (!ct.IsCancellationRequested)
        {
            var line = await ConsoleIO.ReadLineAsync(ct);
            if (line is null) break;

            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length == 0) continue;

            var command = parts[0].ToLowerInvariant();
            var args = parts[1..];

            try
            {
                if (!await DispatchAsync(command, args, ct))
                    return;
            }
            catch (CommandException ex)
            {
                ConsoleIO.WriteError($"[CMD] {ex.Message}");
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                ConsoleIO.WriteError($"[CMD] {command} failed: {ex.Message}");
            }
        }
    }

    /// <summary>Returns false when the loop should terminate.</summary>
    private async Task<bool> DispatchAsync(string command, string[] args, CancellationToken ct)
    {
        switch (command)
        {
            // ---------------- PMD subscriptions ----------------
            case "lvsub":   await pmd.SubscribeLocalViewAsync(RequireAreas(args, command), ct); break;
            case "lvunsub": await pmd.UnsubscribeLocalViewAsync(RequireAreas(args, command), ct); break;
            case "dasub":   await pmd.SubscribeDeliveryAreaAsync(ct); break;
            case "daunsub": await pmd.UnsubscribeDeliveryAreaAsync(ct); break;
            case "csub":    await pmd.SubscribeContractAsync(ct); break;
            case "cunsub":  await pmd.UnsubscribeContractAsync(ct); break;
            case "tsub":    await pmd.SubscribeTickerAsync(ct); break;
            case "tunsub":  await pmd.UnsubscribeTickerAsync(ct); break;
            case "pssub":   await pmd.SubscribePublicStatisticAsync(RequireAreas(args, command), ct); break;
            case "psunsub": await pmd.UnsubscribePublicStatisticAsync(RequireAreas(args, command), ct); break;
            case "h2hsub":   await pmd.SubscribeH2HCapacityAsync(RequireCapacityFlows(args, command), ct); break;
            case "h2hunsub": await pmd.UnsubscribeH2HCapacityAsync(RequireCapacityFlows(args, command), ct); break;
            case "atcsub":   await pmd.SubscribeAtcCapacityAsync(RequireCapacityFlows(args, command), ct); break;
            case "atcunsub": await pmd.UnsubscribeAtcCapacityAsync(RequireCapacityFlows(args, command), ct); break;

            // ---------------- Trading subscriptions ----------------
            case "oersub":   await trading.SubscribeOrderExecutionReportAsync(OptionalInt(args, command), ct); break;
            case "oerunsub": await trading.UnsubscribeOrderExecutionReportAsync(ct); break;
            case "ptsub":    await trading.SubscribePrivateTradeAsync(OptionalInt(args, command), ct); break;
            case "ptunsub":  await trading.UnsubscribePrivateTradeAsync(ct); break;
            case "tlsub":    await trading.SubscribeThrottlingLimitAsync(ct); break;
            case "tlunsub":  await trading.UnsubscribeThrottlingLimitAsync(ct); break;
            case "cfgsub":   await trading.SubscribeConfigurationAsync(ct); break;
            case "cfgunsub": await trading.UnsubscribeConfigurationAsync(ct); break;

            // ---------------- Trading requests ----------------
            case "order":
            {
                var entry = _orders.BuildOrderEntry(ParseOverrides(args), out var clientOrderId);
                await trading.SendOrderEntryAsync(entry, clientOrderId, ct);
                break;
            }
            case "modify":
            {
                if (args.Length == 0) throw new CommandException("Usage: modify <orderId|clientOrderId|last> [qty=..] [price=..] [tif=..] [expire=<min>] [text=..]");
                await trading.SendModificationAsync(_orders.BuildModify(args[0], ParseOverrides(args[1..])), ct);
                break;
            }
            case "deac":
            {
                if (args.Length == 0) throw new CommandException("Usage: deac <orderId|clientOrderId|last>");
                await trading.SendModificationAsync(_orders.BuildDeactivate(args[0]), ct);
                break;
            }
            case "badorder":
            {
                var entry = _orders.BuildInvalidOrderEntry(out var clientOrderId);
                await trading.SendOrderEntryAsync(entry, clientOrderId, ct);
                break;
            }
            case "badmodify":
                await trading.SendModificationAsync(_orders.BuildInvalidModify(), ct);
                break;

            // ---------------- Misc ----------------
            case "hb":
            {
                var target = args.Length > 0 ? args[0].ToLowerInvariant() : "all";
                if (target is "pmd" or "all") await pmd.SendHeartbeatAsync(ct);
                if (target is "trd" or "all") await trading.SendHeartbeatAsync(ct);
                if (target is not ("pmd" or "trd" or "all")) throw new CommandException("Usage: hb [pmd|trd]");
                break;
            }
            case "help":
            case "?":
                PrintHelp();
                break;
            case "quit":
            case "exit":
            case "q":
                await Task.WhenAll(
                    pmd.CloseAsync("User quit", CancellationToken.None),
                    trading.CloseAsync("User quit", CancellationToken.None));
                return false;

            default:
                ConsoleIO.WriteError($"[CMD] Unknown command '{command}'. Type 'help' for a list of commands.");
                break;
        }

        return true;
    }

    // -------------------------------------------------------------------------
    // Argument parsing
    // -------------------------------------------------------------------------

    private static int[] RequireAreas(string[] args, string command)
    {
        if (args.Length == 0)
            throw new CommandException($"Usage: {command} <areaId> [areaId ...]");

        return args.Select(a => int.TryParse(a, out var id)
            ? id
            : throw new CommandException($"Invalid delivery area id '{a}'.")).ToArray();
    }

    /// <summary>Token format: <c>area</c> or <c>area:connected1,connected2</c>.</summary>
    private static List<CapacityFlow> RequireCapacityFlows(string[] args, string command)
    {
        if (args.Length == 0)
            throw new CommandException($"Usage: {command} <area[:connected,...]> [area[:connected,...] ...]");

        var flows = new List<CapacityFlow>();
        foreach (var token in args)
        {
            var split = token.Split(':', 2);
            if (!int.TryParse(split[0], out var area))
                throw new CommandException($"Invalid delivery area id '{split[0]}'.");

            var flow = new CapacityFlow { DeliveryArea = area };
            if (split.Length == 2 && split[1].Length > 0)
            {
                foreach (var c in split[1].Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    if (!int.TryParse(c, out var connected))
                        throw new CommandException($"Invalid connected delivery area id '{c}'.");
                    flow.ConnectedDeliveryAreas.Add(connected);
                }
            }
            flows.Add(flow);
        }

        return flows;
    }

    private static int? OptionalInt(string[] args, string command)
    {
        if (args.Length == 0) return null;
        return int.TryParse(args[0], out var v) && v >= 0
            ? v
            : throw new CommandException($"Usage: {command} [snapshotSize]");
    }

    private static Dictionary<string, string> ParseOverrides(string[] args)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var arg in args)
        {
            var idx = arg.IndexOf('=');
            if (idx <= 0 || idx == arg.Length - 1)
                throw new CommandException($"Expected key=value but got '{arg}'.");
            result[arg[..idx]] = arg[(idx + 1)..];
        }
        return result;
    }

    // -------------------------------------------------------------------------
    // Help
    // -------------------------------------------------------------------------

    private static void PrintHelp()
    {
        ConsoleIO.Write("""

            Public market data (PMD) commands:
              lvsub <areas>          Subscribe to local view order books for delivery areas (e.g. lvsub 10 11)
              lvunsub <areas>        Unsubscribe local view
              dasub | daunsub        Delivery areas
              csub | cunsub          Contracts (required before 'order' can auto-pick a contract)
              tsub | tunsub          Ticker (public trades)
              pssub | psunsub <areas>  Public statistics for delivery areas
              h2hsub | h2hunsub <area[:conn,...]>  Hub-to-hub capacities
              atcsub | atcunsub <area[:conn,...]>  ATC capacities

            Trading commands:
              oersub [n]             Subscribe to order execution reports (optional snapshot size)
              oerunsub               Unsubscribe order execution reports
              ptsub [n]              Subscribe to private trades (optional snapshot size)
              ptunsub                Unsubscribe private trades
              tlsub | tlunsub        Throttling limits
              cfgsub | cfgunsub      Configuration / user profile (required before 'order' can auto-pick a portfolio)
              order [k=v ...]        Enter an order. Defaults: random tradable contract & portfolio, SELL LIMIT GFS qty=3000 price=2500.
                                     Keys: side=buy|sell qty= price= contract= portfolio= area= market=local|xbid
                                           tif=ioc|fok|gtd|gfs expire=<minutes, GTD only> state=acti|hibe text=
              modify <id> [k=v ...]  Modify a cached order (<id> = order_id, client_order_id or 'last'). Keys: qty price tif expire text
              deac <id>              Deactivate a cached order
              badorder               Send an intentionally invalid order entry (demonstrates TradingAck errors)
              badmodify              Send a modify for a non-existent order (demonstrates ITEM_NOT_FOUND)

            General:
              hb [pmd|trd]           Send a heartbeat (both connections by default)
              help | ?               Show this help
              quit | exit | q        Close both connections and exit

            """);
    }
}
