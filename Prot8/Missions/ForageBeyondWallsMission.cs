using Prot8.Constants;
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
    const int AmbushCasualties = 5;
    const int AmbushDeaths = 2;
    const int AmbushWounded = 3;
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
        return $"+{HighFoodGain} Food ({highChance}%) | +{MediumFoodGain} Food ({mediumChance}%) | {AmbushCasualties} Casualties, +{AmbushUnrest} Unrest ({failChance}%)";
    }

    public bool CanStart(GameState state)
    {
        return true;
    }

    public void ResolveOutcome(GameState state, ActiveMission mission, ResolutionEntry entry)
    {
        var (highChance, mediumChance) = GetChances(state);

        var roll = state.RollPercent();
        if (roll <= highChance)
        {
            state.AddResource(ResourceKind.Food, HighFoodGain, entry);
            entry.Write($"The foragers returned with a bountiful haul — wild game and edible roots. The granary swells with {HighFoodGain} food.");
            return;
        }

        if (roll <= highChance + mediumChance)
        {
            state.AddResource(ResourceKind.Food, MediumFoodGain, entry);
            entry.Write($"The foragers found some food in the wild lands, though not as much as hoped. {MediumFoodGain} food is better than nothing.");
            return;
        }

        state.ApplyDeath(AmbushDeaths, entry);
        state.ApplyWounding(AmbushWounded, entry);
        state.AddUnrest(AmbushUnrest, entry);
        entry.Write($"Enemy scouts ambushed the foraging party. Only a handful return — {AmbushCasualties} casualties in the fields. The city mourns.");
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
