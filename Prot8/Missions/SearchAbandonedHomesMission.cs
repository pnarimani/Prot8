using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Missions;

public sealed class SearchAbandonedHomesMission : IMissionDefinition
{
    const int BaseMaterialsChance = 45;
    const int BaseMedicineChance = 35;
    const int MaterialsGain = 40;
    const int MedicineGain = 25;
    const int SuccessSickness = 5;
    const int PlagueSickness = 15;
    const int PlagueDeaths = 2;
    const int LowSicknessBonus = 5;
    const int LowSicknessThreshold = 20;

    public string Id => "search_abandoned_homes";
    public string Name => "Search Abandoned Homes";
    public int DurationDays => 2;
    public int WorkerCost => 4;

    public string GetTooltip(GameState state)
    {
        var (materialsChance, medicineChance) = GetChances(state);
        var failChance = 100 - materialsChance - medicineChance;
        return $"+{MaterialsGain} Materials, +{SuccessSickness} Sickness ({materialsChance}%) | +{MedicineGain} Medicine, +{SuccessSickness} Sickness ({medicineChance}%) | +{PlagueSickness} Sickness, {PlagueDeaths} Deaths ({failChance}%)";
    }

    public bool CanStart(GameState state, out string reason)
    {
        reason = "";
        return true;
    }

    public void ResolveOutcome(GameState state, ActiveMission mission, ResolutionEntry entry)
    {
        var (materialsChance, medicineChance) = GetChances(state);
        var roll = state.RollPercent();
        if (roll <= materialsChance)
        {
            state.AddResource(ResourceKind.Materials, MaterialsGain, entry);
            state.AddSickness(SuccessSickness, entry);
            entry.Write($"{Name}: recovered +{MaterialsGain} materials (+{SuccessSickness} sickness).");
            return;
        }

        if (roll <= materialsChance + medicineChance)
        {
            state.AddResource(ResourceKind.Medicine, MedicineGain, entry);
            state.AddSickness(SuccessSickness, entry);
            entry.Write($"{Name}: recovered +{MedicineGain} medicine (+{SuccessSickness} sickness).");
            return;
        }

        state.AddSickness(PlagueSickness, entry);
        state.ApplyDeath(PlagueDeaths, entry);
        entry.Write($"{Name}: plague exposure (+{PlagueSickness} sickness, {PlagueDeaths} deaths).");
    }

    (int materialsChance, int medicineChance) GetChances(GameState state)
    {
        var matChance = BaseMaterialsChance;
        var medChance = BaseMedicineChance;
        if (state.Sickness < LowSicknessThreshold)
        {
            matChance += LowSicknessBonus;
        }

        return (matChance, medChance);
    }
}
