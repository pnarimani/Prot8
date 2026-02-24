using Prot8.Constants;
using Prot8.Resources;
using Prot8.Zones;

namespace Prot8.Simulation;

public static class StateChangeApplier
{
    extension(GameState state)
    {
        public void AddResource(ResourceKind kind, int amount, ResolutionEntry entry)
        {
            if (amount == 0)
            {
                return;
            }

            if (amount > 0)
            {
                state.Resources.Add(kind, amount);
                entry.Write($"+{amount} {kind}");
            }

            var consumed = state.Resources.Consume(kind, -amount);
            entry.Write($"-{consumed} {kind}");
        }

        public void AddMorale(int amount, ResolutionEntry entry)
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
                entry.Write($"{(applied > 0 ? "+" : string.Empty)}{applied} Morale");
            }
        }

        public void AddUnrest(int amount, ResolutionEntry entry)
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
                entry.Write($"{(applied > 0 ? "+" : string.Empty)}{applied} Unrest");
            }
        }

        public void ApplyGuardDesertion(int deserters)
        {
            if (deserters == 0)
            {
                return;
            }

            state.Population.Guards = Math.Max(0, state.Population.Guards - deserters);
        }

        public void LoseZone(ZoneId zoneId, bool isControlledEvacuation, ResolutionEntry entry)
        {
            var zone = state.GetZone(zoneId);
            if (zone.IsLost)
            {
                return;
            }

            zone.IsLost = true;
            zone.Integrity = 0;

            state.DayFirstZoneLost ??= state.Day;

            state.ZoneLossOccurred = true;

            entry.Write($"Zone Lost: {zone.Name}");
            entry.Write($"Active perimiter is now {state.ActivePerimeterZone.Name}");

            if (isControlledEvacuation)
            {
                state.AddUnrest(GameBalance.EvacuationUnrestShock[zoneId], entry);
                state.AddMorale(-GameBalance.EvacuationMoraleShock[zoneId], entry);
                state.AddSickness(GameBalance.EvacuationSicknessShock[zoneId], entry);
                state.AddResource(ResourceKind.Materials, -GameBalance.EvacuationMaterialsPenalty[zoneId], entry);
            }
            else
            {
                entry.Write("Sudden loss of the zone has caused chaos and panic.");
                state.AddUnrest(GameBalance.NaturalLossUnrestShock[zoneId], entry);
                state.AddMorale(-GameBalance.NaturalLossMoraleShock[zoneId], entry);
                state.AddSickness(GameBalance.NaturalLossSicknessShock[zoneId], entry);
            }
        }

        public void AddSickness(int amount, ResolutionEntry entry)
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
                entry.Write($"{(applied > 0 ? "+" : string.Empty)}{applied} Sickness.");
            }
        }

        public void ApplyDeath(int deathsRequested, ResolutionEntry entry)
        {
            if (deathsRequested <= 0)
            {
                return;
            }

            var applied = state.Population.RemovePeopleByPriority(deathsRequested);
            if (applied > 0)
            {
                state.TotalDeaths += applied;
                state.Allocation.RemoveWorkersProportionally(applied);
                entry.Write($"{applied} deaths");
            }
        }

        public int ApplyWorkerDesertion(int desertersRequested)
        {
            if (desertersRequested <= 0)
            {
                return 0;
            }

            var applied = state.Population.RemoveHealthyWorkers(desertersRequested);
            if (applied > 0)
            {
                state.TotalDesertions += applied;
                state.Allocation.RemoveWorkersProportionally(applied);
            }

            return applied;
        }

        public void ConvertHealthyToSick(int amount, ResolutionEntry entry)
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

            state.Allocation.RemoveWorkersProportionally(converted);
            var recoveryDays = GameBalance.ComputeRecoveryDays(state.Sickness);
            state.Population.AddSickWorkers(converted, recoveryDays);
            entry.Write($"{converted} workers became sick (recovery base {recoveryDays} days).");
        }

        public void RecoverSickWorkers(int amount, DayResolutionReport report, ResolutionEntry entry)
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

            report.RecoveredWorkersToday += recovered;
            state.TotalRecoveredWorkers += recovered;
            entry.Write($"{recovered} sick workers recovered to healthy.");
        }
    }
}