using Prot8.Buildings;
using Prot8.Characters;
using Prot8.Constants;
using Prot8.Defenses;
using Prot8.Missions;
using Prot8.Population;
using Prot8.Resources;
using Prot8.Scavenging;
using Prot8.Zones;

namespace Prot8.Simulation;

public sealed class GameState
{
    public GameState(int? seed = null)
    {
        Random = seed.HasValue ? new Random(seed.Value) : new Random();
        RandomSeed = seed;
        Zones = CreateZones();
        var storages = CreateZoneStorages();
        Resources = new ResourceState(storages);
        Population = new PopulationState
        {
            HealthyWorkers = GameBalance.StartingHealthyWorkers,
            Guards = GameBalance.StartingGuards,
            SickWorkers = GameBalance.StartingSickWorkers,
            Elderly = GameBalance.StartingElderly,
            WoundedWorkers = 0,
        };
        Morale = GameBalance.StartingMorale;
        Unrest = GameBalance.StartingUnrest;
        Sickness = GameBalance.StartingSickness;
        SiegeIntensity = GameBalance.StartingSiegeIntensity;
        Buildings = CreateBuildings();
        Allocation = new BuildingAllocation(Buildings);

        // Distribute starting resources via Add (fills safest first)
        Resources.Add(ResourceKind.Food, GameBalance.StartingFood);
        Resources.Add(ResourceKind.Water, GameBalance.StartingWater);
        Resources.Add(ResourceKind.Fuel, GameBalance.StartingFuel);
        Resources.Add(ResourceKind.Medicine, GameBalance.StartingMedicine);
        Resources.Add(ResourceKind.Materials, GameBalance.StartingMaterials);

        // Ensure first render shows current auto-allocation modes pre-assigned.
        WorkerAllocationStrategy.ApplyAutomaticAllocation(this);

        Population.EnqueueRecovery(Population.SickWorkers, GameBalance.ComputeRecoveryDays(Sickness));

        if (GameBalance.EnableNamedCharacters)
        {
            NamedCharacters = CharacterRoster.CreateStartingCharacters();
        }

        if (GameBalance.EnableReliefArmy)
        {
            ActualReliefDay = GameBalance.ReliefArmyBaseDay + Random.Next(-GameBalance.ReliefArmyVariance, GameBalance.ReliefArmyVariance + 1);
            ReliefEstimateMin = GameBalance.ReliefStartEstimateMin;
            ReliefEstimateMax = GameBalance.ReliefStartEstimateMax;
        }
    }

    public int Day { get; set; } = 1;

    public ResourceState Resources { get; }

    public PopulationState Population { get; }

    public IReadOnlyList<ZoneState> Zones { get; }

    public IReadOnlyList<BuildingState> Buildings { get; }

    public BuildingAllocation Allocation { get; set; }

    public int Morale { get; set; }

    public int Unrest { get; set; }

    public int Sickness { get; set; }

    public int SiegeIntensity { get; set; }

    public int SiegeEscalationDelayDays { get; set; }

    public int LastLawDay { get; set; } = int.MinValue;

    public Dictionary<string, int> OrderCooldowns { get; } = new();

    public string? FirstLawName { get; set; }

    public int? FirstLawDay { get; set; }

    public int TotalDeaths { get; set; }

    public int TotalDesertions { get; set; }

    public int TotalRecoveredWorkers { get; set; }

    public int? DayFirstFoodDeficit { get; set; }

    public int? DayFirstWaterDeficit { get; set; }

    public int? DayFirstZoneLost { get; set; }

    public int ConsecutiveFoodDeficitDays { get; set; }

    public int ConsecutiveWaterDeficitDays { get; set; }

    public int ConsecutiveBothFoodWaterZeroDays { get; set; }

    public bool ZoneLossOccurred { get; set; }

    public bool GameOver { get; set; }

    public GameOverCause GameOverCause { get; set; }

    public string? GameOverDetails { get; set; }

    public bool Survived { get; set; }

    public FlagState Flags { get; } = new();

    public TemporaryDailyEffects DailyEffects { get; set; } = new();

    public List<string> ActiveLawIds { get; } = new();

    public string? ActiveOrderId { get; set; }

    public List<ActiveMission> ActiveMissions { get; } = new();

    public Dictionary<string, int> EventCooldowns { get; } = new();

    public Dictionary<string, int> MissionCooldowns { get; } = new();

    public double SiegeDamageMultiplier { get; set; } = 1.0;

    public int SiegeDamageReductionDaysRemaining { get; set; }

    public bool PlagueRatsActive { get; set; }

    public int TaintedWellDaysRemaining { get; set; }

    public List<ResourceKind> ResourcePriority { get; set; } = new(GameBalance.DefaultResourcePriority);

    public ClinicSpecialization ClinicSpecialization { get; set; } = ClinicSpecialization.None;

    public KitchenRecipe ActiveKitchenRecipe { get; set; } = KitchenRecipe.Normal;

    public int IntelBuffDaysRemaining { get; set; }

    public bool IntelWarningPending { get; set; }

    public Dictionary<ZoneId, ZoneDefenses> ZoneDefenseMap { get; } = new();

    public ZoneDefenses GetZoneDefenses(ZoneId zoneId)
    {
        if (!ZoneDefenseMap.TryGetValue(zoneId, out var defenses))
        {
            defenses = new ZoneDefenses();
            ZoneDefenseMap[zoneId] = defenses;
        }
        return defenses;
    }

    public bool FinalAssaultActive { get; set; }

    public string? ActiveDisruption { get; set; }

    public bool FoodDeficitYesterday { get; set; }

    public bool WaterDeficitYesterday { get; set; }

    public bool FoodDeficitToday { get; set; }

    public bool WaterDeficitToday { get; set; }

    public bool FuelDeficitToday { get; set; }

    public Random Random { get; }

    public int? RandomSeed { get; }

    public int LastDayFoodConsumed { get; set; }

    public int LastDayWaterConsumed { get; set; }

    public int DeathsAtStartOfDay { get; set; }

    public DefensivePosture CurrentPosture { get; set; } = DefensivePosture.None;

    public Dictionary<Zones.ZoneId, bool> ScorchedPerimeterUsed { get; } = new();

    public int ScorchedPerimeterDamageReductionDays { get; set; }

    public bool AreGuardsCommitted =>
        CurrentPosture is DefensivePosture.ActiveDefense or DefensivePosture.AggressivePatrols;

    public Dictionary<Buildings.BuildingId, BuildingSpecialization> BuildingSpecializations { get; } = new();

    public bool EmergencyWaterReserveUsed { get; set; }

    public BuildingSpecialization GetBuildingSpec(Buildings.BuildingId id)
    {
        return BuildingSpecializations.TryGetValue(id, out var spec) ? spec : BuildingSpecialization.None;
    }

    public List<string> ActiveDiplomacyIds { get; } = new();

    public int HostageExchangeDayCounter { get; set; }

    public bool TradingPostBuilt { get; set; }

    public List<NamedCharacter> NamedCharacters { get; set; } = new();

    // Relief Army / Hope Timer
    public int ActualReliefDay { get; set; }
    public int ReliefEstimateMin { get; set; }
    public int ReliefEstimateMax { get; set; }
    public int ReliefAcceleration { get; set; }
    public int ReliefIntelGathered { get; set; }
    public int CorrespondenceAccelerationApplied { get; set; }
    public bool SignalFireLit { get; set; }

    // Night Phase / Scavenging
    public List<ScavengingLocation> AvailableScavengingLocations { get; set; } = new();
    public int ScavengingRefreshDay { get; set; }
    public int FatiguedWorkerCount { get; set; }
    public ScavengingResult? LastNightResult { get; set; }

    // Streak counters for Good Day Morale Boost
    public int ConsecutiveNoDeficitDays { get; set; }
    public int ConsecutiveLowSicknessDays { get; set; }
    public int ConsecutiveZoneHeldDays { get; set; }
    public int ConsecutiveMissionSuccesses { get; set; }

    public IEnumerable<NamedCharacter> LivingCharacters() =>
        NamedCharacters.Where(c => c.IsAlive && !c.HasDeserted);

    public NamedCharacter? GetLivingCharacterWithTrait(CharacterTrait trait) =>
        NamedCharacters.FirstOrDefault(c => c.IsAlive && !c.HasDeserted && c.Trait == trait);

    public List<Trading.TradeOffer> StandingTrades { get; } = new();

    public int TradingPostTradeCount { get; set; }

    public ZoneState ActivePerimeterZone
    {
        get
        {
            foreach (var zone in Zones)
            {
                if (!zone.IsLost)
                {
                    return zone;
                }
            }

            return GetZone(ZoneId.Keep);
        }
    }

    public int ReservedWorkersForMissions
    {
        get
        {
            var reserved = 0;
            foreach (var mission in ActiveMissions)
            {
                reserved += mission.WorkerCost;
            }

            return reserved;
        }
    }

    public int ReservedGuardsForMissions
    {
        get
        {
            var reserved = 0;
            foreach (var mission in ActiveMissions)
            {
                reserved += mission.GuardCost;
            }

            return reserved;
        }
    }

    public int AvailableHealthyWorkersForAllocation
    {
        get
        {
            var available = Population.HealthyWorkers - ReservedWorkersForMissions;
            return available < 0 ? 0 : available;
        }
    }

    public int IdleWorkers
    {
        get
        {
            var sum = Allocation.TotalAssigned();
            var allocation = AvailableHealthyWorkersForAllocation - sum;
            return allocation >= 0
                ? allocation
                : throw new InvalidOperationException("There are more workers assigned to buildings than available workers");
        }
    }

    public ZoneState GetZone(ZoneId zoneId)
    {
        foreach (var zone in Zones)
        {
            if (zone.Id == zoneId)
            {
                return zone;
            }
        }

        throw new KeyNotFoundException($"Zone not found: {zoneId}");
    }

    public bool IsZoneLost(ZoneId zoneId)
    {
        return GetZone(zoneId).IsLost;
    }

    public int CountLostZones()
    {
        var count = 0;
        foreach (var zone in Zones)
        {
            if (zone.IsLost)
            {
                count++;
            }
        }

        return count;
    }

    public int GetZonePopulation()
    {
        var nonLostCount = Zones.Count(x => !x.IsLost);
        return nonLostCount == 0 ? 0 : Population.TotalPopulation / nonLostCount;
    }

    public BuildingState GetBuilding(BuildingId id)
    {
        foreach (var b in Buildings)
        {
            if (b.Id == id)
                return b;
        }
        throw new KeyNotFoundException($"Building not found: {id}");
    }

    public IEnumerable<BuildingState> GetActiveBuildings()
    {
        foreach (var b in Buildings)
        {
            if (!b.IsDestroyed)
                yield return b;
        }
    }

    public IEnumerable<BuildingState> GetBuildingsInZone(ZoneId zone)
    {
        foreach (var b in Buildings)
        {
            if (b.Zone == zone)
                yield return b;
        }
    }

    static IReadOnlyList<ZoneStorageState> CreateZoneStorages()
    {
        var list = new List<ZoneStorageState>();
        foreach (var template in GameBalance.ZoneTemplates)
        {
            list.Add(new ZoneStorageState(template.ZoneId));
        }
        return list;
    }

    static IReadOnlyList<ZoneState> CreateZones()
    {
        var list = new List<ZoneState>();
        foreach (var template in GameBalance.ZoneTemplates)
        {
            list.Add(new ZoneState(template.ZoneId, template.Name, template.StartingIntegrity,
                template.StartingCapacity));
        }

        return list;
    }

    static IReadOnlyList<BuildingState> CreateBuildings()
    {
        var list = new List<BuildingState>();
        foreach (var def in GameBalance.BuildingDefinitions)
        {
            var building = new BuildingState(def);
            if (def.Id == Prot8.Buildings.BuildingId.TradingPost)
            {
                building.IsActive = false;
            }
            list.Add(building);
        }
        return list;
    }

    public int RollPercent()
    {
        return Random.Next(1, 101);
    }
}
