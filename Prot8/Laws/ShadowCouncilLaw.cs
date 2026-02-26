using Prot8.Simulation;

namespace Prot8.Laws;

public sealed class ShadowCouncilLaw : ILaw
{
    private const int DailyUnrestReduction = 3;
    private const double ProductionBonus = 1.05;
    private const int DailyDeaths = 1;
    private const int MoraleCap = 30;

    public string Id => "shadow_council";
    public string Name => "Shadow Council";

    public string GetTooltip(GameState state) =>
        $"-{DailyUnrestReduction} unrest/day, +5% production, 1 death/day (dissenters vanish). Morale capped at {MoraleCap}. Requires Tyranny >= 5 and Iron Fist.";

    public bool CanEnact(GameState state)
    {
        if (state.Flags.Faith >= 3)
        {
            return false;
        }

        if (state.Flags.Tyranny < 5)
        {
            return false;
        }

        if (!state.Flags.IronFist)
        {
            return false;
        }

        return true;
    }

    public void OnEnact(GameState state, ResolutionEntry entry)
    {
        state.Flags.Tyranny.Add(2);
        entry.Write("A council of masked figures now governs from the shadows. Dissenters vanish in the night. Order is absolute.");
    }

    public void ApplyDaily(GameState state, ResolutionEntry entry)
    {
        state.AddUnrest(-DailyUnrestReduction, entry);
        state.DailyEffects.ProductionMultiplier.Apply("Shadow Council", ProductionBonus);
        state.ApplyDeath(DailyDeaths, entry);

        if (state.Morale > MoraleCap)
        {
            var reduction = state.Morale - MoraleCap;
            state.AddMorale(-reduction, entry);
        }

        entry.Write("Another name struck from the rolls. The shadow council ensures obedience.");
    }
}
