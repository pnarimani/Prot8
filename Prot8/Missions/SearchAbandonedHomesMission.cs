using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Missions;

public sealed class SearchAbandonedHomesMission : IMissionDefinition
{
    const int MaterialsChance = 45;
    const int MedicineChance = 35;
    const int MaterialsGain = 40;
    const int MedicineGain = 25;
    const int SuccessSickness = 5;
    const int PlagueSickness = 15;
    const int PlagueDeaths = 2;

    public string Id => "search_abandoned_homes";
    public string Name => "Search Abandoned Homes";
    public int DurationDays => 2;
    public int WorkerCost => 4;

    public string GetTooltip(GameState state)
    {
        var failChance = 100 - MaterialsChance - MedicineChance;
        return $"+{MaterialsGain} Materials, +{SuccessSickness} Sickness ({MaterialsChance}%) | +{MedicineGain} Medicine, +{SuccessSickness} Sickness ({MedicineChance}%) | +{PlagueSickness} Sickness, {PlagueDeaths} Deaths ({failChance}%)";
    }

    public bool CanStart(GameState state, out string reason)
    {
        reason = "";
        return true;
    }

    public void ResolveOutcome(GameState state, ActiveMission mission, DayResolutionReport report)
    {
        var roll = state.RollPercent();
        if (roll <= MaterialsChance)
        {
            StateChangeApplier.AddResource(state, ResourceKind.Materials, MaterialsGain, report, ReasonTags.Mission, Name);
            StateChangeApplier.AddSickness(state, SuccessSickness, report, ReasonTags.Mission, $"{Name} exposure");
            report.AddResolvedMission($"{Name}: recovered +{MaterialsGain} materials (+{SuccessSickness} sickness).");
            return;
        }

        if (roll <= MaterialsChance + MedicineChance)
        {
            StateChangeApplier.AddResource(state, ResourceKind.Medicine, MedicineGain, report, ReasonTags.Mission, Name);
            StateChangeApplier.AddSickness(state, SuccessSickness, report, ReasonTags.Mission, $"{Name} exposure");
            report.AddResolvedMission($"{Name}: recovered +{MedicineGain} medicine (+{SuccessSickness} sickness).");
            return;
        }

        StateChangeApplier.AddSickness(state, PlagueSickness, report, ReasonTags.Mission, Name);
        StateChangeApplier.ApplyDeaths(state, PlagueDeaths, report, ReasonTags.Mission, $"{Name} plague");
        report.AddResolvedMission($"{Name}: plague exposure (+{PlagueSickness} sickness, {PlagueDeaths} deaths).");
    }
}
