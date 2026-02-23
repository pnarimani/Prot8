using Prot8.Simulation;

namespace Prot8.Events;

public sealed class BetrayalFromWithinEvent : TriggeredEventBase, IRespondableEvent
{
    private const int TriggerDay = 37;
    private const int LowGuardThreshold = 5;

    public BetrayalFromWithinEvent() : base("betrayal_within", "Betrayal from Within",
        "A conspiracy among the guards is uncovered. A third of your guards have been plotting to defect.")
    {
    }

    public override bool ShouldTrigger(GameState state)
    {
        return state.Day == TriggerDay;
    }

    public override void Apply(GameState state, DayResolutionReport report)
    {
        ApplyResponse("let_go", state, report);
    }

    public IReadOnlyList<EventResponse> GetResponses(GameState state)
    {
        return
        [
            new EventResponse("amnesty", "Offer amnesty"),
            new EventResponse("example", "Make an example"),
            new EventResponse("let_go", "Let them go"),
        ];
    }

    public void ApplyResponse(string responseId, GameState state, DayResolutionReport report)
    {
        var defectors = Math.Max(1, state.Population.Guards / 3);

        switch (responseId)
        {
            case "amnesty":
            {
                var actual = Math.Min(state.Population.Guards, defectors);
                state.Population.Guards -= actual;
                state.Population.HealthyWorkers += actual;
                StateChangeApplier.AddMorale(state, 5, report, ReasonTags.Event, Name);
                report.Add(ReasonTags.Event, $"{Name}: You offer amnesty. {actual} guards rejoin as workers. Mercy earns grudging respect.");
                break;
            }

            case "example":
            {
                var actual = Math.Min(state.Population.Guards, defectors);
                state.Population.Guards -= actual;
                state.Population.HealthyWorkers += Math.Max(0, actual - 2);
                StateChangeApplier.ApplyDeaths(state, 2, report, ReasonTags.Event, Name);
                StateChangeApplier.AddUnrest(state, 10, report, ReasonTags.Event, Name);
                report.Add(ReasonTags.Event, $"{Name}: Two ringleaders are publicly executed. The rest are stripped of rank. Fear keeps order — for now.");
                break;
            }

            default: // let_go
            {
                StateChangeApplier.ApplyDesertions(state, defectors, report, ReasonTags.Event, Name);
                if (state.Population.Guards < LowGuardThreshold)
                {
                    StateChangeApplier.AddUnrest(state, 15, report, ReasonTags.Event, $"{Name} panic");
                    report.Add(ReasonTags.Event, $"{Name}: The conspirators leave freely. With so few guards remaining, unrest surges.");
                }
                else
                {
                    report.Add(ReasonTags.Event, $"{Name}: The conspirators leave freely. The garrison is weakened.");
                }
                break;
            }
        }

        StartCooldown(state);
    }
}
