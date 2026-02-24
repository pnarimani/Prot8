using Prot8.Simulation;

namespace Prot8.Events;

public sealed class FeverOutbreakEvent : TriggeredEventBase
{
    private const int SicknessThreshold = 60;
    private const int Deaths = 10;
    private const int UnrestGain = 10;

    public FeverOutbreakEvent() : base("fever_outbreak", "Fever Outbreak",
        $"Triggers when sickness > {SicknessThreshold}. {Deaths} deaths, +{UnrestGain} unrest.")
    {
    }

    public override bool ShouldTrigger(GameState state)
    {
        return state.Sickness > SicknessThreshold;
    }

    public override void Apply(GameState state, ResolutionEntry entry)
    {
        entry.Write("The clinic overflows. Corpses line the halls. The stench of death fills every corner.");
        state.ApplyDeath(Deaths, entry);
        state.AddUnrest(UnrestGain, entry);
        StartCooldown(state);
    }
}