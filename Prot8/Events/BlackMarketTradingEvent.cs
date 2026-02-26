using Prot8.Constants;
using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Events;

public sealed class BlackMarketTradingEvent : IRespondableEvent
{
    public string Id => "black_market_trading";
    public string Name => "Black Market Trader";

    public string Description =>
        "A shadowy figure appears at the gate, offering to trade supplies. The goods are real, but the prices are steep — and haggling may stir trouble.";

    public bool ShouldTrigger(GameState state)
    {
        if (!GameBalance.EnableBlackMarketEvent)
            return false;

        if (state.Day < GameBalance.BlackMarketMinDay)
            return false;

        // Use dynamic cooldown from EventCooldowns (set on each trigger)
        // The cooldown system handles recurrence
        return true;
    }

    public void ResolveNow(GameState state, ResolutionEntry entry)
    {
        ResolveWithResponse("refuse", state, entry);
    }

    public IReadOnlyList<EventResponse> GetResponses(GameState state)
    {
        var (give, giveAmount, receive, receiveAmount) = GetTradeOffer(state);
        return
        [
            new EventResponse("accept", $"Accept: Give {giveAmount} {give} for {receiveAmount} {receive}"),
            new EventResponse("haggle", $"Haggle: Give {giveAmount / 2} {give} for {receiveAmount} {receive} (+{GameBalance.BlackMarketHaggleUnrest} Unrest)"),
            new EventResponse("refuse", "Refuse the offer"),
        ];
    }

    public void ResolveWithResponse(string responseId, GameState state, ResolutionEntry entry)
    {
        var (give, giveAmount, receive, receiveAmount) = GetTradeOffer(state);

        // Set dynamic cooldown for next occurrence
        var nextCooldown = state.Random.Next(GameBalance.BlackMarketRecurrenceMin, GameBalance.BlackMarketRecurrenceMax + 1);
        state.EventCooldowns[Id] = nextCooldown;

        switch (responseId)
        {
            case "accept":
                var consumed = state.Resources.Consume(give, giveAmount);
                if (consumed < giveAmount)
                {
                    entry.Write($"You don't have enough {give}. The trader scoffs and leaves.");
                    state.Resources.Add(give, consumed);
                    return;
                }
                state.Resources.Add(receive, receiveAmount);
                entry.Write($"The trader accepts. You exchanged {giveAmount} {give} for {receiveAmount} {receive}.");
                break;

            case "haggle":
                var haggleGive = giveAmount / 2;
                var haggleConsumed = state.Resources.Consume(give, haggleGive);
                if (haggleConsumed < haggleGive)
                {
                    entry.Write($"You don't have enough {give}. The trader scoffs and leaves.");
                    state.Resources.Add(give, haggleConsumed);
                    return;
                }
                state.Resources.Add(receive, receiveAmount);
                state.AddUnrest(GameBalance.BlackMarketHaggleUnrest, entry);
                entry.Write($"After tense bargaining, you traded {haggleGive} {give} for {receiveAmount} {receive}. Word spreads of the unsavory deal.");
                break;

            default: // refuse
                entry.Write("You turn the trader away. He vanishes into the shadows.");
                break;
        }
    }

    static (ResourceKind give, int giveAmount, ResourceKind receive, int receiveAmount) GetTradeOffer(GameState state)
    {
        var seed = state.Day * 31;
        var tradeIndex = seed % 4;
        return tradeIndex switch
        {
            0 => (ResourceKind.Food, 30, ResourceKind.Materials, 20),
            1 => (ResourceKind.Water, 25, ResourceKind.Materials, 20),
            2 => (ResourceKind.Materials, 20, ResourceKind.Food, 30),
            _ => (ResourceKind.Food, 20, ResourceKind.Medicine, 10),
        };
    }
}
