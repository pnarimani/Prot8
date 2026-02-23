using Prot8.Simulation;

namespace Prot8.Events;

public sealed class EnemyUltimatumEvent : TriggeredEventBase
{
    private const int TriggerDay = 30;
    private const int UnrestGain = 20;
    private const int MoraleLoss = 15;
    private const int Desertions = 5;

    public EnemyUltimatumEvent() : base("enemy_ultimatum", "Enemy Ultimatum",
        $"Triggers on day {TriggerDay}. +{UnrestGain} unrest, -{MoraleLoss} morale, {Desertions} desertions.")
    {
    }

    public override bool ShouldTrigger(GameState state)
    {
        return state.Day == TriggerDay;
    }

    public override void Apply(GameState state, DayResolutionReport report)
    {
        StateChangeApplier.AddUnrest(state, UnrestGain, report, ReasonTags.Event, Name);
        StateChangeApplier.AddMorale(state, -MoraleLoss, report, ReasonTags.Event, Name);
        StateChangeApplier.ApplyDesertions(state, Desertions, report, ReasonTags.Event, $"{Name} panic");

        report.Add(ReasonTags.Event, $"{Name}: the enemy demands surrender. Civilians question whether resistance is worth the cost.");
        StartCooldown(state);
    }
}
