using Prot8.Constants;
using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Trading;

public static class TradingEngine
{
    public static double GetBaseRate(GameState state)
    {
        return state.SiegeIntensity >= 4 ? GameBalance.TradingPostHighSiegeRate : GameBalance.TradingPostBaseRate;
    }

    public static double GetEffectiveRate(GameState state)
    {
        var baseRate = GetBaseRate(state);

        // Daily fluctuation based on day seed
        var fluctuation = 1.0 + (state.Random.NextDouble() * 2 - 1) * GameBalance.TradingPostFluctuationRange;
        var rate = baseRate * fluctuation;

        // Tyranny path discount
        if (state.Flags.Tyranny >= 3)
            rate = Math.Min(rate, GameBalance.TradingPostTyrannyRate);

        return rate;
    }

    public static void ResolveTrades(GameState state, DayResolutionReport report)
    {
        if (!GameBalance.EnableTradingPost || !state.TradingPostBuilt)
            return;

        var tradingPost = state.GetBuilding(Buildings.BuildingId.TradingPost);
        if (tradingPost.IsDestroyed)
            return;

        var maxTrades = tradingPost.AssignedWorkers;
        if (maxTrades <= 0 || state.StandingTrades.Count == 0)
            return;

        var tradesExecuted = 0;
        var entry = new ResolutionEntry { Title = "Trading Post" };
        var rate = GetEffectiveRate(state);

        foreach (var trade in state.StandingTrades)
        {
            if (tradesExecuted >= maxTrades)
                break;

            var inputNeeded = (int)Math.Ceiling(trade.Amount * rate);
            if (inputNeeded <= 0)
                inputNeeded = 1;

            if (!state.Resources.Has(trade.SourceResource, inputNeeded))
            {
                entry.Write($"Trade {trade.SourceResource}->{trade.TargetResource}: insufficient {trade.SourceResource} (need {inputNeeded}).");
                continue;
            }

            // Interception check
            var interceptionChance = GameBalance.TradingPostInterceptionBase * state.SiegeIntensity;
            if (GameBalance.EnableFlagSystem && state.Flags.Faith >= 3)
                interceptionChance /= 2;

            if (state.RollPercent() <= interceptionChance)
            {
                state.Resources.Consume(trade.SourceResource, inputNeeded);
                entry.Write($"Trade intercepted! Lost {inputNeeded} {trade.SourceResource} to enemy raids.");
                tradesExecuted++;
                continue;
            }

            state.Resources.Consume(trade.SourceResource, inputNeeded);
            var output = trade.Amount;
            state.Resources.Add(trade.TargetResource, output);
            entry.Write($"Traded {inputNeeded} {trade.SourceResource} for {output} {trade.TargetResource} (rate {rate:F1}:1).");
            tradesExecuted++;

            // Faith path bonus
            if (GameBalance.EnableFlagSystem && state.Flags.Faith >= 3 && state.RollPercent() <= GameBalance.TradingPostFaithBonusChance)
            {
                var resourceKinds = new[] { ResourceKind.Food, ResourceKind.Water, ResourceKind.Fuel, ResourceKind.Materials, ResourceKind.Medicine };
                var bonusKind = resourceKinds[state.Random.Next(0, resourceKinds.Length)];
                state.Resources.Add(bonusKind, GameBalance.TradingPostFaithBonusAmount);
                entry.Write($"Faithful traders include a gift: +{GameBalance.TradingPostFaithBonusAmount} {bonusKind}.");
            }

            // Tyranny path unrest tracking
            if (state.Flags.Tyranny >= 3)
            {
                state.TradingPostTradeCount++;
                if (state.TradingPostTradeCount % GameBalance.TradingPostTyrannyUnrestInterval == 0)
                {
                    state.AddUnrest(GameBalance.TradingPostTyrannyUnrest, entry);
                    entry.Write("Forced trade terms anger the population.");
                }
            }
        }

        if (entry.Messages.Count > 0)
            report.Entries.Add(entry);
    }
}
