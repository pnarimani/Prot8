using Prot8.Simulation;

namespace Prot8.Events;

public sealed class BetrayalFromWithinEvent() : TriggeredEventBase("betrayal_within", "Betrayal from Within",
        "A conspiracy among the guards is uncovered. A third of your guards have been plotting to defect."),
    IRespondableEvent
{
    const int TriggerDay = 37;
    const int LowGuardThreshold = 5;

    public override bool ShouldTrigger(GameState state)
    {
        return state.Day == TriggerDay;
    }

    public override void Apply(GameState state, ResolutionEntry entry)
    {
        ApplyResponse("let_go", state, entry);
    }

    public IReadOnlyList<EventResponse> GetResponses(GameState state)
    {
        return
        [
            new EventResponse("amnesty", "Offer amnesty"),
            new EventResponse("example", "Make an example", "Kill the 2 ringleaders"),
            new EventResponse("let_go", "Let them go"),
        ];
    }

    public void ApplyResponse(string responseId, GameState state, ResolutionEntry entry)
    {
        var defectors = Math.Max(1, state.Population.Guards / 3);

        switch (responseId)
        {
            case "amnesty":
            {
                var actual = Math.Min(state.Population.Guards, defectors);
                state.Population.Guards -= actual;
                state.Population.HealthyWorkers += actual;
                entry.Write($"You offer amnesty. {actual} guards rejoin as workers. Mercy earns grudging respect.");
                state.AddMorale(5, entry);
                break;
            }

            case "example":
            {
                var actual = Math.Min(state.Population.Guards, defectors);
                state.Population.Guards -= actual;
                state.Population.HealthyWorkers += Math.Max(0, actual - 2);

                entry.Write(
                    $"You make an example of the ringleaders. {Math.Min(2, actual)} are executed, the rest are demoted to workers. Fear keeps order.");
                state.ApplyDeath(2, entry);
                state.AddUnrest(10, entry);
                break;
            }

            default: // let_go
            {
                entry.Write("The conspirators leave freely");
                state.ApplyGuardDesertion(defectors);
                if (state.Population.Guards < LowGuardThreshold)
                {
                    entry.Write("With so few guards remaining, unrest surges.");
                    state.AddUnrest(15, entry);
                }
                else
                {
                    entry.Write("The garrison is weakened, but order is maintained.");
                }

                break;
            }
        }

        StartCooldown(state);
    }
}