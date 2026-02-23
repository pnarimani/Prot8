using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Events;

public sealed class WellContaminationScareEvent : TriggeredEventBase
{
    private const int TriggerDay = 5;
    private const int SicknessHit = 5;
    private const int ReducedSicknessHit = 2;
    private const int MedicineCost = 5;

    public WellContaminationScareEvent() : base("well_contamination", "Well Contamination Scare",
        "Day 5: Well contamination suspected. +5 sickness (or +2 if medicine >= 5, auto-spends 5 medicine).")
    {
    }

    public override bool ShouldTrigger(GameState state)
    {
        return state.Day == TriggerDay;
    }

    public override void Apply(GameState state, DayResolutionReport report)
    {
        if (state.Resources.Has(ResourceKind.Medicine, MedicineCost))
        {
            StateChangeApplier.AddResource(state, ResourceKind.Medicine, -MedicineCost, report, ReasonTags.Event, Name);
            StateChangeApplier.AddSickness(state, ReducedSicknessHit, report, ReasonTags.Event, Name);
            report.Add(ReasonTags.Event, $"{Name}: Medicine purifies the wells. The worst is averted — for now.");
        }
        else
        {
            StateChangeApplier.AddSickness(state, SicknessHit, report, ReasonTags.Event, Name);
            report.Add(ReasonTags.Event, $"{Name}: Without medicine to treat the wells, sickness spreads through the water.");
        }

        StartCooldown(state);
    }
}
