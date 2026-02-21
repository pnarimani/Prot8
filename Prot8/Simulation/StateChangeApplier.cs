using Prot8.Constants;
using Prot8.Population;
using Prot8.Resources;
using Prot8.Zones;

namespace Prot8.Simulation;

public static class StateChangeApplier
{
    public static void AddResource(GameState state, ResourceKind kind, int amount, DayResolutionReport report, string tag, string reason)
    {
        if (amount == 0)
        {
            return;
        }

        if (amount > 0)
        {
            state.Resources.Add(kind, amount);
            report.Add(tag, $"{reason}: +{amount} {kind}.");
            return;
        }

        var consumed = state.Resources.Consume(kind, -amount);
        report.Add(tag, $"{reason}: -{consumed} {kind}.");
    }

    public static void AddMorale(GameState state, int amount, DayResolutionReport report, string tag, string reason)
    {
        if (amount == 0)
        {
            return;
        }

        var before = state.Morale;
        state.Morale = GameBalance.ClampStat(state.Morale + amount);
        var applied = state.Morale - before;
        if (applied != 0)
        {
            report.Add(tag, $"{reason}: {(applied > 0 ? "+" : string.Empty)}{applied} Morale.");
        }
    }

    public static void AddUnrest(GameState state, int amount, DayResolutionReport report, string tag, string reason)
    {
        if (amount == 0)
        {
            return;
        }

        var before = state.Unrest;
        state.Unrest = GameBalance.ClampStat(state.Unrest + amount);
        var applied = state.Unrest - before;
        if (applied != 0)
        {
            report.Add(tag, $"{reason}: {(applied > 0 ? "+" : string.Empty)}{applied} Unrest.");
        }
    }

    public static void AddSickness(GameState state, int amount, DayResolutionReport report, string tag, string reason)
    {
        if (amount == 0)
        {
            return;
        }

        var before = state.Sickness;
        state.Sickness = GameBalance.ClampStat(state.Sickness + amount);
        var applied = state.Sickness - before;
        if (applied != 0)
        {
            report.Add(tag, $"{reason}: {(applied > 0 ? "+" : string.Empty)}{applied} Sickness.");
        }
    }

    public static int ApplyDeaths(GameState state, int deathsRequested, DayResolutionReport report, string tag, string reason)
    {
        if (deathsRequested <= 0)
        {
            return 0;
        }

        var applied = state.Population.RemovePeopleByPriority(deathsRequested);
        if (applied > 0)
        {
            state.TotalDeaths += applied;
            state.RebalanceHousing();
            report.Add(tag, $"{reason}: {applied} deaths.");
        }

        return applied;
    }

    public static int ApplyDesertions(GameState state, int desertersRequested, DayResolutionReport report, string tag, string reason)
    {
        if (desertersRequested <= 0)
        {
            return 0;
        }

        var applied = state.Population.RemoveHealthyWorkers(desertersRequested);
        if (applied > 0)
        {
            state.TotalDesertions += applied;
            state.RebalanceHousing();
            report.Add(tag, $"{reason}: {applied} workers deserted.");
        }

        return applied;
    }

    public static void ConvertHealthyToSick(GameState state, int amount, DayResolutionReport report, string reason)
    {
        if (amount <= 0)
        {
            return;
        }

        var converted = state.Population.RemoveHealthyWorkers(amount);
        if (converted <= 0)
        {
            return;
        }

        var recoveryDays = GameBalance.ComputeRecoveryDays(state.Sickness);
        state.Population.AddSickWorkers(converted, recoveryDays);
        state.RebalanceHousing();
        report.Add(ReasonTags.Sickness, $"{reason}: {converted} workers became sick (recovery base {recoveryDays} days).");
    }

    public static void RecoverSickWorkers(GameState state, int amount, DayResolutionReport report, string reason)
    {
        if (amount <= 0)
        {
            return;
        }

        var recovered = state.Population.RecoverWorkers(amount);
        if (recovered <= 0)
        {
            return;
        }

        state.RebalanceHousing();
        report.RecoveredWorkersToday += recovered;
        state.TotalRecoveredWorkers += recovered;
        report.Add(ReasonTags.RecoveryComplete, $"{reason}: {recovered} sick workers recovered to healthy.");
    }

    public static void LoseZone(GameState state, ZoneId zoneId, bool isControlledEvacuation, DayResolutionReport report)
    {
        var zone = state.GetZone(zoneId);
        if (zone.IsLost)
        {
            return;
        }

        zone.IsLost = true;
        zone.Integrity = 0;

        if (!state.DayFirstZoneLost.HasValue)
        {
            state.DayFirstZoneLost = state.Day;
        }

        state.ZoneLossOccurred = true;
        state.RebalanceHousing();

        if (isControlledEvacuation)
        {
            AddUnrest(state, GameBalance.EvacuationUnrestShock[zoneId], report, ReasonTags.ZoneLoss, $"Evacuated {zone.Name}");
            AddMorale(state, -GameBalance.EvacuationMoraleShock[zoneId], report, ReasonTags.ZoneLoss, $"Evacuated {zone.Name}");
            AddSickness(state, GameBalance.EvacuationSicknessShock[zoneId], report, ReasonTags.ZoneLoss, $"Evacuated {zone.Name}");
            AddResource(state, ResourceKind.Materials, -GameBalance.EvacuationMaterialsPenalty[zoneId], report, ReasonTags.ZoneLoss, $"Evacuation loss from {zone.Name}");
        }
        else
        {
            AddUnrest(state, GameBalance.NaturalLossUnrestShock[zoneId], report, ReasonTags.ZoneLoss, $"{zone.Name} fell");
            AddMorale(state, -GameBalance.NaturalLossMoraleShock[zoneId], report, ReasonTags.ZoneLoss, $"{zone.Name} fell");
            AddSickness(state, GameBalance.NaturalLossSicknessShock[zoneId], report, ReasonTags.ZoneLoss, $"{zone.Name} fell");
        }

        report.Add(ReasonTags.ZoneLoss, $"Zone lost: {zone.Name}. Active perimeter is now {state.ActivePerimeterZone.Name}.");
    }
}
