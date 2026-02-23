using Prot8.Simulation;

namespace Prot8.Events;

public sealed class DespairEvent : TriggeredEventBase
{
    public DespairEvent() : base("despair", "Wave of Despair",
        "After day 10, 15% daily chance when morale < 45. Sudden morale and unrest shock.")
    {
    }

    public override bool ShouldTrigger(GameState state)
    {
        if (state.Day < 10)
        {
            return false;
        }

        if (state.Morale >= 45)
        {
            return false;
        }

        return state.Random.Next(1, 101) <= 15;
    }

    public override void Apply(GameState state, DayResolutionReport report)
    {
        StateChangeApplier.AddMorale(state, -10, report, ReasonTags.Event, Name);
        StateChangeApplier.AddUnrest(state, 8, report, ReasonTags.Event, Name);
        StateChangeApplier.ApplyDesertions(state, 3, report, ReasonTags.Event, $"{Name} desertions");

        StartCooldown(state);
    }
}
