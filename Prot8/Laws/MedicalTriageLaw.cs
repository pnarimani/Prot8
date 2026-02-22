using Prot8.Simulation;

namespace Prot8.Laws;

public sealed class MedicalTriageLaw : LawBase
{
    private const double MedicineUsageMultiplier = 0.5;
    private const int DailySickDeaths = 5;
    private const int MedicineThreshold = 20;

    public MedicalTriageLaw() : base("medical_triage", "Medical Triage", $"-{MedicineUsageMultiplier * 100}% medicine usage, +{DailySickDeaths} sick deaths/day. Requires medicine < {MedicineThreshold}.")
    {
    }

    public override string GetDynamicTooltip(GameState state) => $"-{MedicineUsageMultiplier * 100}% medicine usage, +{DailySickDeaths} sick deaths/day. Requires medicine < {MedicineThreshold}.";

    public override bool CanEnact(GameState state, out string reason)
    {
        if (state.Resources[Resources.ResourceKind.Medicine] < 20)
        {
            reason = string.Empty;
            return true;
        }

        reason = "Requires medicine below 20.";
        return false;
    }

    public override void ApplyDaily(GameState state, DayResolutionReport report)
    {
        state.DailyEffects.MedicineUsageMultiplier *= MedicineUsageMultiplier;
        var deaths = state.Population.RemoveSickWorkers(DailySickDeaths);
        if (deaths > 0)
        {
            state.TotalDeaths += deaths;
            state.RebalanceHousing();
            report.Add(ReasonTags.LawPassive, $"{Name}: {deaths} sick workers died due to triage limits.");
        }
    }
}