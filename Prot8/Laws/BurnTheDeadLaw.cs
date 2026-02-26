using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Laws;

public sealed class BurnTheDeadLaw : ILaw
{
    const int SicknessReduction = 15;
    const int FuelCost = 2;
    const int MoraleHit = 10;
    const int SicknessThreshold = 35;

    public string Id => "burn_the_dead";
    public string Name => "Burn the Dead";

    public string GetTooltip(GameState state)
    {
        return $"-{SicknessReduction} sickness, -{FuelCost} fuel, -{MoraleHit} morale";
    }

    public bool CanEnact(GameState state)
    {
        if (state.Flags.FaithRisen)
        {
            return false;
        }

        if (state.Sickness > SicknessThreshold)
        {
            return true;
        }

        return false;
    }

    public void OnEnact(GameState state, ResolutionEntry entry)
    {
        state.Flags.Tyranny.Add(1);
        state.Flags.MercyDenied.Set();
        entry.Write(
            "Pyres burn day and night. The stench of cremation fills the air. The dead find no proper burial, but at least they no longer spread plague.");
        state.AddMorale(-MoraleHit, entry);
        state.AddResource(ResourceKind.Fuel, -FuelCost, entry);
        state.AddSickness(-SicknessReduction, entry);
    }

    public void ApplyDaily(GameState state, ResolutionEntry entry)
    {
    }
}