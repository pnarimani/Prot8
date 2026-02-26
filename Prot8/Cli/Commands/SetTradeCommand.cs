using System.Text.Json.Serialization;
using Prot8.Constants;
using Prot8.Resources;
using Prot8.Simulation;
using Prot8.Trading;

namespace Prot8.Cli.Commands;

public sealed class SetTradeCommand : ICommand
{
    [JsonPropertyName("source")]
    public required string SourceStr { get; init; }

    [JsonPropertyName("target")]
    public required string TargetStr { get; init; }

    [JsonPropertyName("amount")]
    public required int Amount { get; init; }

    public CommandResult Execute(CommandContext context)
    {
        if (!GameBalance.EnableTradingPost)
            return new CommandResult(false, "Trading post is not enabled.");

        var state = context.State;

        if (!state.TradingPostBuilt)
            return new CommandResult(false, "Trading post has not been built yet.");

        if (Amount <= 0)
        {
            // Clear all trades
            state.StandingTrades.Clear();
            return new CommandResult(true, "All standing trades cleared.");
        }

        if (!TryParseResource(SourceStr, out var source))
            return new CommandResult(false, $"Unknown resource '{SourceStr}'.");

        if (!TryParseResource(TargetStr, out var target))
            return new CommandResult(false, $"Unknown resource '{TargetStr}'.");

        if (source == target)
            return new CommandResult(false, "Cannot trade a resource for itself.");

        state.StandingTrades.Add(new TradeOffer(source, target, Amount));
        return new CommandResult(true, $"Standing trade set: {Amount} {target} for {source}.");
    }

    static bool TryParseResource(string input, out ResourceKind result)
    {
        result = input.ToLowerInvariant() switch
        {
            "food" => ResourceKind.Food,
            "water" => ResourceKind.Water,
            "fuel" => ResourceKind.Fuel,
            "materials" => ResourceKind.Materials,
            "medicine" => ResourceKind.Medicine,
            _ => (ResourceKind)(-1),
        };
        return (int)result >= 0;
    }
}
