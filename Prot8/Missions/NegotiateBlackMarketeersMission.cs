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

    public void ResolveOutcome(GameState state, ActiveMission mission, DayResolutionReport report)
    {
        var roll = state.RollPercent();
        if (roll <= WaterChance)
        {
            StateChangeApplier.AddResource(state, ResourceKind.Water, WaterGain, report, ReasonTags.Mission, Name);
            StateChangeApplier.AddUnrest(state, SuccessUnrest, report, ReasonTags.Mission, $"{Name} corruption");
            report.AddResolvedMission($"{Name}: acquired +{WaterGain} water (+{SuccessUnrest} unrest).");
            return;
        }

        if (roll <= WaterChance + FoodChance)
        {
            StateChangeApplier.AddResource(state, ResourceKind.Food, FoodGain, report, ReasonTags.Mission, Name);
            StateChangeApplier.AddUnrest(state, SuccessUnrest, report, ReasonTags.Mission, $"{Name} corruption");
            report.AddResolvedMission($"{Name}: acquired +{FoodGain} food (+{SuccessUnrest} unrest).");
            return;
        }

        StateChangeApplier.AddUnrest(state, BetrayalUnrest, report, ReasonTags.Mission, Name);
        StateChangeApplier.ApplyDeaths(state, BetrayalDeaths, report, ReasonTags.Mission, $"{Name} betrayal");
        report.AddResolvedMission($"{Name}: betrayal (+{BetrayalUnrest} unrest, {BetrayalDeaths} deaths).");
    }
}
