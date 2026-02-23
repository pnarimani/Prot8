using Prot8.Simulation;

namespace Prot8.Events;

public sealed class EnemyUltimatumEvent : TriggeredEventBase, IRespondableEvent
{
    private const int TriggerDay = 30;

    public EnemyUltimatumEvent() : base("enemy_ultimatum", "Enemy Ultimatum",
        "The enemy commander demands your surrender. Civilians question whether resistance is worth the cost.")
    {
    }

    public override bool ShouldTrigger(GameState state)
    {
        return state.Day == TriggerDay;
    }

    public override void Apply(GameState state, DayResolutionReport report)
    {
        ApplyResponse("ignore", state, report);
    }

    public IReadOnlyList<EventResponse> GetResponses(GameState state)
    {
        return
        [
            new EventResponse("defy", "Defy them publicly"),
            new EventResponse("negotiate", "Negotiate for time"),
            new EventResponse("ignore", "Ignore the ultimatum"),
        ];
    }

    public void ApplyResponse(string responseId, GameState state, DayResolutionReport report)
    {
        switch (responseId)
        {
            case "defy":
                StateChangeApplier.AddMorale(state, 10, report, ReasonTags.Event, Name);
                StateChangeApplier.AddUnrest(state, 15, report, ReasonTags.Event, Name);
                report.Add(ReasonTags.Event, $"{Name}: You rally the people with fiery words. Spirits lift, but the hotheads grow bolder.");
                break;

            case "negotiate":
                StateChangeApplier.AddMorale(state, -5, report, ReasonTags.Event, Name);
                StateChangeApplier.AddUnrest(state, 5, report, ReasonTags.Event, Name);
                StateChangeApplier.ApplyDesertions(state, 2, report, ReasonTags.Event, $"{Name} negotiation");
                report.Add(ReasonTags.Event, $"{Name}: You buy time, but the appearance of weakness emboldens deserters.");
                break;

            default: // ignore
                StateChangeApplier.AddMorale(state, -15, report, ReasonTags.Event, Name);
                StateChangeApplier.AddUnrest(state, 20, report, ReasonTags.Event, Name);
                StateChangeApplier.ApplyDesertions(state, 5, report, ReasonTags.Event, $"{Name} panic");
                report.Add(ReasonTags.Event, $"{Name}: Silence is taken as weakness. Panic spreads through the ranks.");
                break;
        }

        StartCooldown(state);
    }
}
