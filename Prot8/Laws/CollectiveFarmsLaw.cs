using Prot8.Simulation;

namespace Prot8.Laws;

public sealed class CollectiveFarmsLaw : ILaw
{
    private const double FoodProductionBonus = 1.30;
    private const int MoraleGain = 5;
    private const int DailyUnrest = 3;

    public string Id => "collective_farms";
    public string Name => "Collective Farms";

    public string GetTooltip(GameState state) =>
        $"+30% food production, +{MoraleGain} morale on enact, +{DailyUnrest} unrest/day (arguments over sharing). Requires Faith >= 2.";

    public bool CanEnact(GameState state, out string reason)
    {
        if (state.Flags.MartialState)
        {
            reason = "Martial authority controls all production.";
            return false;
        }

        if (state.Flags.Faith < 2)
        {
            reason = "Requires a foundation of faith.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    public void OnEnact(GameState state, ResolutionEntry entry)
    {
        state.Flags.Faith.Add(1);
        state.AddMorale(MoraleGain, entry);
        entry.Write("The farms are collectivized. All harvest is shared equally among the people. Arguments erupt over fair portions, but bellies are fuller.");
    }

    public void ApplyDaily(GameState state, ResolutionEntry entry)
    {
        state.DailyEffects.FoodProductionMultiplier.Apply("Collective Farms", FoodProductionBonus);
        state.AddUnrest(DailyUnrest, entry);
        entry.Write("Quarrels break out at the communal granary. Sharing breeds resentment, but no one goes hungry.");
    }
}
