using Prot8.Constants;
using Prot8.Jobs;
using Prot8.Missions;
using Prot8.Population;
using Prot8.Resources;
using Prot8.Zones;

namespace Prot8.Simulation;

public sealed class GameState
{
    public GameState(int? seed = null)
    {
        Random = seed.HasValue ? new Random(seed.Value) : new Random();
        RandomSeed = seed;
        Resources = new ResourceState(GameBalance.StartingFood, GameBalance.StartingWater, GameBalance.StartingFuel,
            GameBalance.StartingMedicine, GameBalance.StartingMaterials);
        Population = new PopulationState
        {
            HealthyWorkers = GameBalance.StartingHealthyWorkers,
            Guards = GameBalance.StartingGuards,
            SickWorkers = GameBalance.StartingSickWorkers,
            Elderly = GameBalance.StartingElderly,
        };
        Morale = GameBalance.StartingMorale;
        Unrest = GameBalance.StartingUnrest;
        Sickness = GameBalance.StartingSickness;
        SiegeIntensity = GameBalance.StartingSiegeIntensity;
        Zones = CreateZones();
        Allocation = new JobAllocation();

        Population.EnqueueRecovery(Population.SickWorkers, GameBalance.ComputeRecoveryDays(Sickness));
    }

    public int Day { get; set; } = 1;

    public ResourceState Resources { get; }

    public PopulationState Population { get; }

    public IReadOnlyList<ZoneState> Zones { get; }

    public JobAllocation Allocation { get; set; }

    public int Morale { get; set; }

    public int Unrest { get; set; }

    public int Sickness { get; set; }

    public int SiegeIntensity { get; set; }

    public int SiegeEscalationDelayDays { get; set; }

    public int LastLawDay { get; set; } = int.MinValue;

    public int LastOrderDay { get; set; } = int.MinValue;

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

    public TemporaryDailyEffects DailyEffects { get; set; } = new();

    public List<string> ActiveLawIds { get; } = new();

    public string? ActiveOrderId { get; set; }

    public List<ActiveMission> ActiveMissions { get; } = new();

    public Dictionary<string, int> EventCooldowns { get; } = new();

    public Dictionary<string, int> MissionCooldowns { get; } = new();

    public double SiegeDamageMultiplier { get; set; } = 1.0;

    public bool PlagueRatsActive { get; set; }

    public bool FoodDeficitYesterday { get; set; }

    public bool WaterDeficitYesterday { get; set; }

    public bool FoodDeficitToday { get; set; }

    public bool WaterDeficitToday { get; set; }

    public bool FuelDeficitToday { get; set; }

    public Random Random { get; }

    public int? RandomSeed { get; }

    public int LastDayFoodConsumed { get; set; }

    public int LastDayWaterConsumed { get; set; }

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
            var sum = Allocation.Workers.Sum(x => x.Value);
            var allocation = AvailableHealthyWorkersForAllocation - sum;
            return allocation >= 0
                ? allocation
                : throw new InvalidOperationException("There are  more workers assigned to jobs than available workers");
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
        return Population.TotalPopulation / Zones.Count(x => !x.IsLost);
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

    public int RollPercent()
    {
        return Random.Next(0, 101);
    }
}