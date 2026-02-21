using System;
using System.Collections.Generic;
using System.Linq;

namespace Prot8.Population;

public sealed class PopulationState
{
    private readonly List<RecoveryCohort> _recoveryQueue = new();

    public PopulationState(int healthyWorkers, int guards, int sickWorkers, int elderly)
    {
        HealthyWorkers = healthyWorkers;
        Guards = guards;
        SickWorkers = sickWorkers;
        Elderly = elderly;
    }

    public int HealthyWorkers { get; private set; }

    public int Guards { get; private set; }

    public int SickWorkers { get; private set; }

    public int Elderly { get; private set; }

    public IReadOnlyList<RecoveryCohort> RecoveryQueue => _recoveryQueue;

    public int TotalPopulation => HealthyWorkers + Guards + SickWorkers + Elderly;

    public int RemovePeopleByPriority(int amount)
    {
        if (amount <= 0)
        {
            return 0;
        }

        var remaining = amount;
        var removedFromSick = 0;

        remaining = RemoveFromPool(remaining, () => HealthyWorkers, value => HealthyWorkers = value);
        var beforeSickRemoval = remaining;
        remaining = RemoveFromPool(remaining, () => SickWorkers, value => SickWorkers = value);
        removedFromSick += beforeSickRemoval - remaining;
        RemoveFromRecoveryQueue(removedFromSick, true);

        remaining = RemoveFromPool(remaining, () => Elderly, value => Elderly = value);
        remaining = RemoveFromPool(remaining, () => Guards, value => Guards = value);

        return amount - remaining;
    }

    public int RemoveHealthyWorkers(int amount)
    {
        if (amount <= 0)
        {
            return 0;
        }

        var removed = Math.Min(HealthyWorkers, amount);
        HealthyWorkers -= removed;
        return removed;
    }

    public int ConvertHealthyToGuards(int amount)
    {
        var converted = RemoveHealthyWorkers(amount);
        Guards += converted;
        return converted;
    }

    public int RemoveSickWorkers(int amount)
    {
        if (amount <= 0)
        {
            return 0;
        }

        var removed = Math.Min(SickWorkers, amount);
        SickWorkers -= removed;
        RemoveFromRecoveryQueue(removed, true);
        return removed;
    }

    public int AddSickWorkers(int amount, int recoveryDays)
    {
        if (amount <= 0)
        {
            return 0;
        }

        SickWorkers += amount;
        EnqueueRecovery(amount, recoveryDays);
        return amount;
    }

    public int RecoverWorkers(int amount)
    {
        if (amount <= 0)
        {
            return 0;
        }

        var recovered = Math.Min(SickWorkers, amount);
        if (recovered <= 0)
        {
            return 0;
        }

        SickWorkers -= recovered;
        HealthyWorkers += recovered;
        RemoveFromRecoveryQueue(recovered, false);
        return recovered;
    }

    public void EnqueueRecovery(int amount, int recoveryDays)
    {
        if (amount <= 0)
        {
            return;
        }

        _recoveryQueue.Add(new RecoveryCohort(amount, recoveryDays));
    }

    public void AdvanceRecoveryTimers()
    {
        foreach (var cohort in _recoveryQueue)
        {
            if (cohort.DaysRemaining > 0)
            {
                cohort.DaysRemaining -= 1;
            }
        }
    }

    public int ReadyToRecoverCount() => _recoveryQueue.Where(c => c.DaysRemaining <= 0).Sum(c => c.Count);

    public int ReservedInRecoveryQueue() => _recoveryQueue.Sum(c => c.Count);

    private int RemoveFromPool(int remaining, Func<int> getter, Action<int> setter)
    {
        if (remaining <= 0)
        {
            return 0;
        }

        var current = getter();
        if (current <= 0)
        {
            return remaining;
        }

        var removed = Math.Min(current, remaining);
        setter(current - removed);
        return remaining - removed;
    }

    private void RemoveFromRecoveryQueue(int amount, bool removeAny)
    {
        if (amount <= 0)
        {
            return;
        }

        var remaining = amount;
        var ordered = removeAny
            ? _recoveryQueue.OrderBy(c => c.DaysRemaining).ToList()
            : _recoveryQueue.Where(c => c.DaysRemaining <= 0).OrderBy(c => c.DaysRemaining).ToList();

        foreach (var cohort in ordered)
        {
            if (remaining <= 0)
            {
                break;
            }

            if (cohort.Count <= 0)
            {
                continue;
            }

            var removed = Math.Min(cohort.Count, remaining);
            cohort.Count -= removed;
            remaining -= removed;
        }

        _recoveryQueue.RemoveAll(c => c.Count <= 0);
    }
}
