using Prot8.Constants;
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
        $"-{(1 - MedicineUsageMultiplier) * 100}% medicine usage, {DailySickDeaths} sick deaths/day, -{DailyMoraleHit} morale/day. Requires medicine < {MedicineThreshold}.";

    public bool CanEnact(GameState state)
    {
        if (state.Flags.PeopleFirst)
        {
            return false;
        }

        if (state.Flags.Tyranny < 2)
        {
            return false;
        }

        if (state.Resources[ResourceKind.Medicine] < MedicineThreshold)
        {
            return true;
        }

        return false;
    }

    public void OnEnact(GameState state, ResolutionEntry entry)
    {
        state.Flags.Tyranny.Add(2);
        state.Flags.MercyDenied.Set();
        if (GameBalance.EnableHumanityScore) state.Flags.Humanity.Add(-10);
        entry.Write("The sick ward becomes a sorting ground. Doctors must choose who receives medicine and who is left to die.");
    }

    public void ApplyDaily(GameState state, ResolutionEntry entry)
    {
        state.DailyEffects.MedicineUsageMultiplier.Apply("Medical Triage", MedicineUsageMultiplier);
        var sickToKill = Math.Min(DailySickDeaths, state.Population.SickWorkers);
        if (sickToKill > 0)
        {
            var removed = state.Population.RemoveSickWorkers(sickToKill);
            if (removed > 0)
            {
                state.TotalDeaths += removed;
                state.Allocation.RemoveWorkersProportionally(removed);
                entry.Write($"Triage claims {removed} more. The clinic staff cannot meet every gaze.");
            }
        }

        state.AddMorale(-DailyMoraleHit, entry);
    }
}
