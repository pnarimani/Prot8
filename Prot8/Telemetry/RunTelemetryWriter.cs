using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Prot8.Simulation;

namespace Prot8.Telemetry;

public sealed class RunTelemetryWriter : IDisposable
{
    private readonly StreamWriter _writer;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = false };

    public RunTelemetryWriter(int? seed)
    {
        Directory.CreateDirectory("runs");
        var seedPart = seed.HasValue ? $"seed{seed.Value}" : "seedrnd";
        var fileName = $"run_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{seedPart}.jsonl";
        FilePath = Path.Combine("runs", fileName);
        _writer = new StreamWriter(File.Open(FilePath, FileMode.Create, FileAccess.Write, FileShare.Read));
    }

    public string FilePath { get; }

    public void LogDay(GameState state, TurnActionChoice action, DayResolutionReport report)
    {
        var record = new
        {
            type = "day",
            day = state.Day,
            action = new
            {
                law_id = action.LawId,
                emergency_order_id = action.EmergencyOrderId,
                mission_id = action.MissionId,
                selected_zone = action.SelectedZoneForOrder?.ToString()
            },
            state = SnapshotState(state),
            report = new
            {
                entries = report.Entries,
                triggered_events = report.TriggeredEvents,
                resolved_missions = report.ResolvedMissions,
                food_deficit_today = report.FoodDeficitToday,
                water_deficit_today = report.WaterDeficitToday,
                fuel_deficit_today = report.FuelDeficitToday,
                overcrowding_stacks_today = report.OvercrowdingStacksToday,
                recovered_workers_today = report.RecoveredWorkersToday,
                recovery_enabled = report.RecoveryEnabledToday,
                recovery_medicine_spent = report.RecoveryMedicineSpentToday,
                recovery_block_reason = report.RecoveryBlockedReason
            }
        };

        WriteRecord(record);
    }

    public void LogFinal(GameState state)
    {
        var summary = new
        {
            type = "summary",
            survived = state.Survived,
            end_day = state.Day,
            cause_of_loss = state.Survived ? "survived_day_40" : state.GameOverCause.ToString(),
            loss_details = state.GameOverDetails,
            day_of_loss = state.Survived ? (int?)null : state.Day,
            day_first_food_deficit = state.DayFirstFoodDeficit,
            day_first_water_deficit = state.DayFirstWaterDeficit,
            day_first_zone_lost = state.DayFirstZoneLost,
            first_law = state.FirstLawName,
            first_law_day = state.FirstLawDay,
            total_deaths = state.TotalDeaths,
            total_desertions = state.TotalDesertions,
            final_unrest = state.Unrest,
            final_morale = state.Morale,
            final_sickness = state.Sickness,
            total_recovered_workers = state.TotalRecoveredWorkers
        };

        WriteRecord(summary);
        _writer.Flush();
    }

    public void Dispose()
    {
        _writer.Dispose();
    }

    private object SnapshotState(GameState state)
    {
        var zones = new List<object>();
        foreach (var zone in state.Zones)
        {
            zones.Add(new
            {
                id = zone.Id.ToString(),
                name = zone.Name,
                integrity = zone.Integrity,
                capacity = zone.Capacity,
                population = zone.Population,
                is_lost = zone.IsLost
            });
        }

        return new
        {
            resources = state.Resources.Snapshot(),
            population = new
            {
                healthy_workers = state.Population.HealthyWorkers,
                guards = state.Population.Guards,
                sick_workers = state.Population.SickWorkers,
                elderly = state.Population.Elderly,
                total = state.Population.TotalPopulation
            },
            morale = state.Morale,
            unrest = state.Unrest,
            sickness = state.Sickness,
            siege_intensity = state.SiegeIntensity,
            siege_delay_days = state.SiegeEscalationDelayDays,
            active_perimeter = state.ActivePerimeterZone.Name,
            zones,
            enacted_laws = state.ActiveLawIds,
            active_missions = state.ActiveMissions,
            consecutive_food_deficit_days = state.ConsecutiveFoodDeficitDays,
            consecutive_water_deficit_days = state.ConsecutiveWaterDeficitDays,
            consecutive_food_water_zero_days = state.ConsecutiveBothFoodWaterZeroDays,
            total_deaths = state.TotalDeaths,
            total_desertions = state.TotalDesertions
        };
    }

    private void WriteRecord(object record)
    {
        var json = JsonSerializer.Serialize(record, _jsonOptions);
        _writer.WriteLine(json);
    }

}
