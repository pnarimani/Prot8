using Prot8.Constants;
using Prot8.Missions;
using Prot8.Population;
using Prot8.Resources;
using Prot8.Zones;

namespace Prot8.Simulation;

public static class StatModifiers
{
    public static TrackedMultiplier ComputeGlobalProductionMultiplier(GameState state)
    {
        var result = new TrackedMultiplier();
        if (!GameBalance.EnableProductionMultipliers)
            return result;

        if (state.Morale < GameBalance.MoraleProductionThreshold)
        {
            var moraleFactor = 0.5 + (state.Morale / (2.0 * GameBalance.MoraleProductionThreshold));
            result.Apply($"Low morale ({state.Morale})", moraleFactor);
        }

        if (state.Unrest > GameBalance.UnrestProductionThreshold)
        {
            var unrestFactor = 1.0 - (state.Unrest / 200.0);
            result.Apply($"Unrest ({state.Unrest})", unrestFactor);
        }

        if (state.Sickness > GameBalance.SicknessProductionThreshold)
        {
            var sicknessFactor = 1.0 - (state.Sickness / 200.0);
            result.Apply($"Sickness ({state.Sickness})", sicknessFactor);
        }

        return result;
    }

    public static TrackedDelta ComputeSicknessFromEnvironment(GameState state)
    {
        var result = new TrackedDelta();
        result.Add("Base drift", 1);

        if (state.Unrest >= 50)
        {
            result.Add("High unrest", 1);
        }

        if (state.Unrest >= 75)
        {
            result.Add("Severe unrest", 1);
        }

        if (state.Resources[Resources.ResourceKind.Fuel] == 0)
        {
            result.Add("No fuel", 2);
        }

        if (state.PlagueRatsActive)
        {
            result.Add("Plague rats", 3);
        }

        return result;
    }

    public static TrackedDelta ComputeUnrestProgression(GameState state)
    {
        var result = new TrackedDelta();
        result.Add("Base drift", 1);

        if (state.Morale < 40)
        {
            result.Add("Low morale", 1);
        }

        if (state.Morale < 25)
        {
            result.Add("Very low morale", 3);
        }

        if (state.Sickness > 50)
        {
            result.Add("High sickness", 1);
        }

        if (state.Sickness > 70)
        {
            result.Add("Severe sickness", 1);
        }

        var lostZones = state.CountLostZones();
        if (lostZones > 0)
        {
            result.Add("Lost zones", lostZones);
        }

        var guardReduction = state.Population.Guards / 5;
        if (guardReduction > 0)
        {
            result.Add("Guards", -guardReduction);
        }

        return result;
    }

    public static TrackedDelta ComputeMoraleDrift(GameState state)
    {
        var result = new TrackedDelta();
        result.Add("Base drift", -1);

        if (state.Unrest > 50)
        {
            result.Add("High unrest", -1);
        }

        if (state.Sickness > 50)
        {
            result.Add("High sickness", -1);
        }

        var lostZones = state.CountLostZones();
        if (lostZones > 0)
        {
            result.Add("Lost zones", -lostZones);
        }

        if (state.Day > 25)
        {
            result.Add("Siege fatigue", -1);
        }

        return result;
    }
}
