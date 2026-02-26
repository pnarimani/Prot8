using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Missions;

public sealed class RaidCivilianFarmsMission : IMissionDefinition
{
    private const int SuccessChance = 60;
    private const int SuccessFood = 60;
    private const int PartialFood = 30;
    private const int PartialUnrest = 15;
    private const int PartialDeaths = 2;

    public string Id => "raid_civilian_farms";
    public string Name => "Raid Civilian Farms";
    public int DurationDays => 2;
    public int WorkerCost => 4;

    public string GetTooltip(GameState state) =>
        $"+{SuccessFood} food ({SuccessChance}%) | +{PartialFood} food, +{PartialUnrest} unrest, {PartialDeaths} deaths ({100 - SuccessChance}%). Requires Tyranny >= 3.";

    public bool CanStart(GameState state)
    {
        if (state.Flags.Tyranny < 3)
        {
            return false;
        }

        return true;
    }

    public void ResolveOutcome(GameState state, ActiveMission mission, ResolutionEntry entry)
    {
        state.Flags.Tyranny.Add(1);
        var roll = state.RollPercent();

        if (roll <= SuccessChance)
        {
            state.AddResource(ResourceKind.Food, SuccessFood, entry);
            entry.Write($"The raid was a success. Granaries plundered, livestock driven back. +{SuccessFood} food — taken from those who had little to begin with.");
            return;
        }

        state.AddResource(ResourceKind.Food, PartialFood, entry);
        state.AddUnrest(PartialUnrest, entry);
        state.ApplyDeath(PartialDeaths, entry);
        entry.Write($"The raid met resistance. Farmers fought back. +{PartialFood} food, but {PartialDeaths} dead and the people whisper of your cruelty.");
    }
}
