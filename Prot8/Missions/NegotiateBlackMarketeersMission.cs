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
            entry.Write($"The negotiators secured water from the black market. The people drink, but whispers of dealings with criminals spread through the streets.");
            return;
        }

        if (roll <= WaterChance + FoodChance)
        {
            state.AddResource(ResourceKind.Food, FoodGain, entry);
            state.AddUnrest(SuccessUnrest, entry);
            entry.Write($"The negotiators returned with food. bellies are full tonight, but the cost of dealing with smugglers breeds contempt.");
            return;
        }

        state.AddUnrest(BetrayalUnrest, entry);
        state.ApplyDeath(BetrayalDeaths, entry);
        entry.Write("Ambush! The black marketeers turned on your people. Two are killed, the rest barely escape. The deal was a trap.");
    }
}
