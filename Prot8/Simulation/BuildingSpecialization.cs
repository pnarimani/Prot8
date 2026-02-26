namespace Prot8.Simulation;

public enum BuildingSpecialization
{
    None,

    // Farm
    GrainSilos,
    MedicinalHerbs,

    // HerbGarden
    ApothecaryLab,
    HealersRefuge,

    // Well
    DeepBoring,
    PurificationBasin,

    // FuelStore
    CoalPits,
    RationedDistribution,

    // FieldKitchen
    SoupLine,
    FortifiedKitchen,

    // Workshop
    ArmsFoundry,
    SalvageYard,

    // Smithy
    WarSmith,
    ArmorWorks,

    // Cistern
    RainCollection,
    EmergencyReserve,

    // Clinic (migrated from ClinicSpecialization)
    Hospital,
    QuarantineWard,

    // Storehouse
    WeaponCache,
    EmergencySupplies,

    // RootCellar
    PreservedStores,
    MushroomFarm,

    // RepairYard
    SiegeWorkshop,
    EngineerCorps,

    // RationingPost
    DistributionHub,
    PropagandaPost,
}
