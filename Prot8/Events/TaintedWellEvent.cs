using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Events;

public sealed class TaintedWellEvent() : ITriggeredEvent
{
    public string Id => "tainted_well";
    public string Name => "Tainted Well";
    public string Description => "The city's primary well has been fouled — whether by enemy sabotage or simple rot. The water runs discoloured and smells of death.";

    private const int TriggerDay = 18;
    private const int WaterLost = 20;
    private const int SicknessGain = 10;
    private const double WaterProductionPenalty = 0.6;
    private const int PenaltyDuration = 3;

    public bool ShouldTrigger(GameState state)
    {
        return state.Day == TriggerDay;
    }

    public void ResolveNow(GameState state, ResolutionEntry entry)
    {
        entry.Write("The well-keeper reports a foul smell from the city's main cistern. Testing confirms contamination. Reserves are drained and the water supply is compromised for days to come.");
        state.AddResource(ResourceKind.Water, -WaterLost, entry);
        state.AddSickness(SicknessGain, entry);
        entry.Write($"Water production will be reduced for {PenaltyDuration} days until the well can be treated.");
        state.TaintedWellDaysRemaining = PenaltyDuration;
    }
}
