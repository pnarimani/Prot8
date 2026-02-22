using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Laws;

public sealed class MedicalTriageLaw : ILaw
{
    const double MedicineUsageMultiplier = 0.5;
    const int DailySickDeaths = 5;
    const int MedicineThreshold = 20;

    public string Id => "medical_triage";
    public string Name => "Medical Triage";

    public string GetTooltip(GameState state) =>
        $"-{MedicineUsageMultiplier * 100}% medicine usage, +{DailySickDeaths} sick deaths/day. Requires medicine < {MedicineThreshold}.";

    public bool CanEnact(GameState state, out string reason)
    {
        if (state.Resources[ResourceKind.Medicine] < 20)
        {
            reason = string.Empty;
            return true;
        }

        reason = "Requires medicine below 20.";
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
    }
}