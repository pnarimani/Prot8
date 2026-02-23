using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Laws;

public sealed class MedicalTriageLaw : ILaw
{
    const double MedicineUsageMultiplier = 0.5;
    const int DailySickDeaths = 3;
    const int DailyMoraleHit = 2;
    const int MedicineThreshold = 20;

    public string Id => "medical_triage";
    public string Name => "Medical Triage";

    public string GetTooltip(GameState state) =>
        $"-{MedicineUsageMultiplier * 100}% medicine usage, {DailySickDeaths} sick deaths/day, -{DailyMoraleHit} morale/day. Requires medicine < {MedicineThreshold}.";

    public bool CanEnact(GameState state, out string reason)
    {
        if (state.Resources[ResourceKind.Medicine] < MedicineThreshold)
        {
            reason = string.Empty;
            return true;
        }

        reason = $"Requires medicine below {MedicineThreshold}.";
        return false;
    }

    public void OnEnact(GameState state, DayResolutionReport report)
    {
    }

    public void ApplyDaily(GameState state, DayResolutionReport report)
    {
        state.DailyEffects.MedicineUsageMultiplier *= MedicineUsageMultiplier;
        var deaths = state.Population.RemoveSickWorkers(DailySickDeaths);
        if (deaths > 0)
        {
            state.TotalDeaths += deaths;
            report.Add(ReasonTags.LawPassive, $"{Name}: {deaths} sick workers died due to triage limits.");
        }

        StateChangeApplier.AddMorale(state, -DailyMoraleHit, report, ReasonTags.LawPassive, Name);
    }
}
