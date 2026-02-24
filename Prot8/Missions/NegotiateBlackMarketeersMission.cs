using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Missions;

public sealed class NegotiateBlackMarketeersMission : IMissionDefinition
{
    const int WaterChance = 45;
    const int FoodChance = 30;
    const int WaterGain = 60;
    const int FoodGain = 50;
    const int SuccessUnrest = 10;
    const int BetrayalUnrest = 25;
    const int BetrayalDeaths = 2;

    public string Id => "negotiate";
    public string Name => "Negotiate Black Marketeers";
    public int DurationDays => 3;
    public int WorkerCost => 2;

    public string GetTooltip(GameState state)
    {
        var failChance = 100 - WaterChance - FoodChance;
        return $"+{WaterGain} Water, +{SuccessUnrest} Unrest ({WaterChance}%) | +{FoodGain} Food, +{SuccessUnrest} Unrest ({FoodChance}%) | +{BetrayalUnrest} Unrest, {BetrayalDeaths} Deaths ({failChance}%)";
    }

    public bool CanStart(GameState state, out string reason)
    {
        reason = "";
        return true;
    }

    public void ResolveOutcome(GameState state, ActiveMission mission, ResolutionEntry entry)
    {
        var roll = state.RollPercent();
        if (roll <= WaterChance)
        {
            state.AddResource(ResourceKind.Water, WaterGain, entry);
            state.AddUnrest(SuccessUnrest, entry);
            entry.Write($"{Name}: acquired +{WaterGain} water (+{SuccessUnrest} unrest).");
            return;
        }

        if (roll <= WaterChance + FoodChance)
        {
            state.AddResource(ResourceKind.Food, FoodGain, entry);
            state.AddUnrest(SuccessUnrest, entry);
            entry.Write($"{Name}: acquired +{FoodGain} food (+{SuccessUnrest} unrest).");
            return;
        }

        state.AddUnrest(BetrayalUnrest, entry);
        state.ApplyDeath(BetrayalDeaths, entry);
        entry.Write($"{Name}: betrayal (+{BetrayalUnrest} unrest, {BetrayalDeaths} deaths).");
    }
}
