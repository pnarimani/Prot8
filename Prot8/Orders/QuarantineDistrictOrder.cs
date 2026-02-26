using Prot8.Simulation;

namespace Prot8.Orders;

public sealed class QuarantineDistrictOrder : IEmergencyOrder
{
    private const int SicknessReduction = 12;
    private const int UnrestReduction = 3;
    private const int SicknessThreshold = 30;

    public string Id => "quarantine_district";
    public string Name => "Quarantine District";
    public int CooldownDays => 3;

    public string GetTooltip(GameState state) =>
        $"Quarantine the active perimeter zone: -50% production in that zone today, -{SicknessReduction} sickness, -{UnrestReduction} unrest. Requires sickness > {SicknessThreshold}.";

    public bool CanIssue(GameState state)
    {
        if (state.Flags.MercyDenied)
        {
            return false;
        }

        if (state.Sickness <= SicknessThreshold)
        {
            return false;
        }

        return true;
    }

    public void Apply(GameState state, ResolutionEntry entry)
    {
        state.Flags.Faith.Add(1);
        var zone = state.ActivePerimeterZone;
        state.DailyEffects.QuarantineZone = zone.Id;
        state.DailyEffects.QuarantineSicknessReduction = SicknessReduction;
        state.AddUnrest(-UnrestReduction, entry);
        entry.Write($"The {zone.Name} is sealed. No one enters, no one leaves. Production halts, but the disease is contained — and the streets feel calmer for it.");
    }
}
