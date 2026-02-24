using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Missions;

public sealed class ForageBeyondWallsMission() : IMissionDefinition
{
    const int HighFoodGain = 80;
    const int MediumFoodGain = 50;
    const int HighChanceNormal = 50;
    const int MediumChanceNormal = 25;
    const int HighChanceSiege = 40;
    const int MediumChanceSiege = 20;
    const int SiegeThreshold = 4;
    const int AmbushDeaths = 5;
    const int AmbushUnrest = 10;
    const int MoraleBonus = 5;
    const int MoraleThreshold = 60;

    public string Id => "forage_beyond_walls";
    public string Name => "Forage Beyond Walls";
    public int DurationDays => 4;
    public int WorkerCost => 5;

    public string GetTooltip(GameState state)
    {
        var (highChance, mediumChance) = GetChances(state);
        var failChance = 100 - highChance - mediumChance;
        return $"+{HighFoodGain} Food ({highChance}%) | +{MediumFoodGain} Food ({mediumChance}%) | {AmbushDeaths} Deaths, +{AmbushUnrest} Unrest ({failChance}%)";
    }

    public bool CanStart(GameState state, out string reason)
    {
        reason = string.Empty;
        return true;
    }

    public void ResolveOutcome(GameState state, ActiveMission mission, ResolutionEntry entry)
    {
        var (highChance, mediumChance) = GetChances(state);

        var roll = state.RollPercent();
        if (roll <= highChance)
        {
            state.AddResource(ResourceKind.Food, HighFoodGain, entry);
            entry.Write($"{Name}: great haul (+{HighFoodGain} food).");
            return;
        }

        if (roll <= highChance + mediumChance)
        {
            state.AddResource(ResourceKind.Food, MediumFoodGain, entry);
            entry.Write($"{Name}: modest haul (+{MediumFoodGain} food).");
            return;
        }

        state.ApplyDeath(AmbushDeaths, entry);
        state.AddUnrest(AmbushUnrest, entry);
        entry.Write($"{Name}: crew ambushed ({AmbushDeaths} deaths, +{AmbushUnrest} unrest).");
    }

    (int highChance, int mediumChance) GetChances(GameState state)
    {
        var (baseHigh, baseMedium) = state.SiegeIntensity >= SiegeThreshold
            ? (HighChanceSiege, MediumChanceSiege)
            : (HighChanceNormal, MediumChanceNormal);

        if (state.Morale >= MoraleThreshold)
        {
            baseHigh += MoraleBonus;
        }

        return (baseHigh, baseMedium);
    }
}
