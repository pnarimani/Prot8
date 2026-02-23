using Prot8.Simulation;

namespace Prot8.Events;

public sealed class EnemyUltimatumEvent : TriggeredEventBase
{
    public EnemyUltimatumEvent() : base("enemy_ultimatum", "Enemy Ultimatum",
        "Triggers once on day 30. Massive unrest and morale shock.")
    {
    }

    public override bool ShouldTrigger(GameState state)
    {
        return state.Day == 30;
    }

    public override void Apply(GameState state, DayResolutionReport report)
    {
        StateChangeApplier.AddUnrest(state, 20, report, ReasonTags.Event, Name);
        StateChangeApplier.AddMorale(state, -15, report, ReasonTags.Event, Name);
        StateChangeApplier.ApplyDesertions(state, 5, report, ReasonTags.Event, $"{Name} panic");

        report.Add(ReasonTags.Event, $"{Name}: the enemy demands surrender. Civilians question whether resistance is worth the cost.");
    }
}
