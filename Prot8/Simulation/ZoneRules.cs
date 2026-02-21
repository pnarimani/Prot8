using Prot8.Constants;
using Prot8.Zones;

namespace Prot8.Simulation;

public static class ZoneRules
{
    public static bool CanEvacuate(GameState state, ZoneId zoneId, out string reason)
    {
        reason = string.Empty;

        if (zoneId == ZoneId.Keep)
        {
            reason = "Keep cannot be evacuated.";
            return false;
        }

        var zone = state.GetZone(zoneId);
        if (zone.IsLost)
        {
            reason = $"{zone.Name} is already lost.";
            return false;
        }

        var allOuterLost = true;
        foreach (var outer in state.Zones)
        {
            if (outer.Id == zoneId)
            {
                break;
            }

            if (!outer.IsLost)
            {
                allOuterLost = false;
                break;
            }
        }

        if (allOuterLost)
        {
            return true;
        }

        if (zone.Integrity < GameBalance.EvacIntegrityThreshold)
        {
            return true;
        }

        if (state.SiegeIntensity >= GameBalance.EvacSiegeThreshold)
        {
            return true;
        }

        reason = $"Evacuation locked: requires all outer zones lost, {zone.Name} integrity < {GameBalance.EvacIntegrityThreshold}, or siege intensity >= {GameBalance.EvacSiegeThreshold}.";
        return false;
    }

    public static double PerimeterFactor(GameState state)
    {
        var active = state.ActivePerimeterZone.Id;
        foreach (var template in GameBalance.ZoneTemplates)
        {
            if (template.ZoneId == active)
            {
                return template.PerimeterFactor;
            }
        }

        return 1.0;
    }
}