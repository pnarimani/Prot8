using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Laws;

public sealed class ScorchedEarthDoctrineLaw : ILaw
{
    private const double SiegeDamageReduction = 0.70;
    private const int MaterialsCost = 20;
    private const int DailyUnrest = 5;

    public string Id => "scorched_earth";
    public string Name => "Scorched Earth Doctrine";

    public string GetTooltip(GameState state) =>
        $"Siege damage -30% permanently. -{MaterialsCost} materials on enact. +{DailyUnrest} unrest/day. Outer zones cannot be evacuated. Requires Fortification >= 6 and Garrison State.";

    public bool CanEnact(GameState state)
    {
        if (state.Flags.Faith >= 4)
        {
            return false;
        }

        if (state.Flags.Fortification < 6)
        {
            return false;
        }

        if (!state.Flags.GarrisonState)
        {
            return false;
        }

        return true;
    }

    public void OnEnact(GameState state, ResolutionEntry entry)
    {
        state.Flags.Fortification.Add(2);
        state.Flags.WallsHold.Set();
        state.SiegeDamageMultiplier *= SiegeDamageReduction;
        state.AddResource(ResourceKind.Materials, -MaterialsCost, entry);
        entry.Write("Everything beyond the inner walls is set ablaze. The enemy will find nothing but ash and stone. The walls will hold — or we all perish within them.");
    }

    public void ApplyDaily(GameState state, ResolutionEntry entry)
    {
        state.AddUnrest(DailyUnrest, entry);
        entry.Write("The people stare at the scorched ruins beyond the walls. Some weep. Others sharpen their blades.");
    }
}
