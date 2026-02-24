using Prot8.Simulation;

namespace Prot8.Events;

public sealed class DespairEvent : TriggeredEventBase
{
    private const int MinimumDay = 10;
    private const int MoraleThreshold = 45;
    private const int TriggerChance = 15;
    private const int MoraleLoss = 10;
    private const int UnrestGain = 8;
    private const int Desertions = 3;

    public DespairEvent() : base("despair", "Wave of Despair",
        $"After day {MinimumDay}, {TriggerChance}% daily chance when morale < {MoraleThreshold}. -{MoraleLoss} morale, +{UnrestGain} unrest, {Desertions} desertions.")
    {
    }

    public override bool ShouldTrigger(GameState state)
    {
        if (state.Day < MinimumDay)
        {
            return false;
        }

        if (state.Morale >= MoraleThreshold)
        {
            return false;
        }

        return state.Random.Next(1, 101) <= TriggerChance;
    }

    public override void Apply(GameState state, ResolutionEntry entry)
    {
        state.AddMorale(-MoraleLoss, entry);
        state.AddUnrest(UnrestGain, entry);

        StartCooldown(state);
    }
}
