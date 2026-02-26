using Prot8.Buildings;

namespace Prot8.Simulation;

public sealed record SpecializationOption(BuildingSpecialization Spec, string Name, string Description);

public static class BuildingSpecializationCatalog
{
    private static readonly Dictionary<BuildingId, SpecializationOption[]> Options = new()
    {
        [BuildingId.Farm] =
        [
            new(BuildingSpecialization.GrainSilos, "Grain Silos", "Output 4 food/worker (up from 3)."),
            new(BuildingSpecialization.MedicinalHerbs, "Medicinal Herbs", "Output 2 food + 0.5 medicine/worker."),
        ],
        [BuildingId.HerbGarden] =
        [
            new(BuildingSpecialization.ApothecaryLab, "Apothecary Lab", "Output 1.5 medicine/worker, requires 1 fuel input."),
            new(BuildingSpecialization.HealersRefuge, "Healer's Refuge", "No output bonus, -3 sickness/day passive."),
        ],
        [BuildingId.Well] =
        [
            new(BuildingSpecialization.DeepBoring, "Deep Boring", "Output 4 water/worker, fuel input 2 (up from 1)."),
            new(BuildingSpecialization.PurificationBasin, "Purification Basin", "No output bonus, -2 sickness/day passive."),
        ],
        [BuildingId.FuelStore] =
        [
            new(BuildingSpecialization.CoalPits, "Coal Pits", "Output 3 fuel/worker, +1 sickness/day."),
            new(BuildingSpecialization.RationedDistribution, "Rationed Distribution", "Output 1.5 fuel/worker, -15% fuel consumption globally."),
        ],
        [BuildingId.FieldKitchen] =
        [
            new(BuildingSpecialization.SoupLine, "Soup Line", "Output 3 food/worker, -3 morale/day."),
            new(BuildingSpecialization.FortifiedKitchen, "Fortified Kitchen", "Survives zone loss (rebuilt in next inner zone). +5 morale on spec."),
        ],
        [BuildingId.Workshop] =
        [
            new(BuildingSpecialization.ArmsFoundry, "Arms Foundry", "Output 3 materials/worker, requires 1 fuel input."),
            new(BuildingSpecialization.SalvageYard, "Salvage Yard", "No output bonus, 10% daily chance of +5 random resource."),
        ],
        [BuildingId.Smithy] =
        [
            new(BuildingSpecialization.WarSmith, "War Smith", "Output 2 integrity/worker, input 3 materials (up from 2)."),
            new(BuildingSpecialization.ArmorWorks, "Armor Works", "No integrity change. +2 guards on spec, Fortification +1."),
        ],
        [BuildingId.Cistern] =
        [
            new(BuildingSpecialization.RainCollection, "Rain Collection", "Output 1.5 water/worker, doubles on Heavy Rains."),
            new(BuildingSpecialization.EmergencyReserve, "Emergency Reserve", "+20 water on spec, auto-releases 10 water if water hits 0 (once)."),
        ],
        [BuildingId.Clinic] =
        [
            new(BuildingSpecialization.Hospital, "Hospital", "+50% recovery slots."),
            new(BuildingSpecialization.QuarantineWard, "Quarantine Ward", "-5 sickness/day."),
        ],
        [BuildingId.Storehouse] =
        [
            new(BuildingSpecialization.WeaponCache, "Weapon Cache", "No fuel output, instead -5 unrest/day. Tyranny +1."),
            new(BuildingSpecialization.EmergencySupplies, "Emergency Supplies", "No change, 50% resource salvage on zone loss."),
        ],
        [BuildingId.RootCellar] =
        [
            new(BuildingSpecialization.PreservedStores, "Preserved Stores", "Output 1.5 food/worker, -10% food consumption globally."),
            new(BuildingSpecialization.MushroomFarm, "Mushroom Farm", "Output 2 food/worker, +1 sickness/day."),
        ],
        [BuildingId.RepairYard] =
        [
            new(BuildingSpecialization.SiegeWorkshop, "Siege Workshop", "Output 2 integrity/worker, input 4 materials (up from 3)."),
            new(BuildingSpecialization.EngineerCorps, "Engineer Corps", "No change, fortification upgrades cost 50% less materials."),
        ],
        [BuildingId.RationingPost] =
        [
            new(BuildingSpecialization.DistributionHub, "Distribution Hub", "Output 1.5 water/worker, -5% food consumption globally."),
            new(BuildingSpecialization.PropagandaPost, "Propaganda Post", "No water output, +3 morale/day, -2 unrest/day. Faith +1."),
        ],
    };

    public static IReadOnlyList<SpecializationOption> GetOptions(BuildingId buildingId)
    {
        return Options.TryGetValue(buildingId, out var options) ? options : [];
    }

    public static bool IsValidFor(BuildingId buildingId, BuildingSpecialization spec)
    {
        if (!Options.TryGetValue(buildingId, out var options))
            return false;
        foreach (var opt in options)
        {
            if (opt.Spec == spec)
                return true;
        }
        return false;
    }
}
