using Prot8.Simulation;

namespace Prot8.Events;

public sealed class FinalAssaultEvent : TriggeredEventBase
{
    private const int TriggerDay = 33;
    private const int UnrestGain = 15;
    private const int MoraleLoss = 10;

    public FinalAssaultEvent() : base("final_assault", "Final Assault Begins",
        "Day 33: Siege damage x1.5 for remaining game. +15 unrest, -10 morale.")
    {
    }

    public override bool ShouldTrigger(GameState state)
    {
        return state.Day == TriggerDay;
    }

    public override void Apply(GameState state, ResolutionEntry entry)
    {
        state.FinalAssaultActive = true;
        state.AddUnrest(UnrestGain, entry);

        entry.Write($"{Name}: the enemy commits all forces. Siege damage permanently increased.");
        StartCooldown(state);
    }
}
