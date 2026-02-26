using Prot8.Constants;
using Prot8.Zones;

namespace Prot8.Simulation;

public static class ZoneRules
{
    public static bool CanEvacuate(GameState state, ZoneId zoneId)
    {
        if (zoneId == ZoneId.Keep)
        {
            return false;
        }

        var zone = state.GetZone(zoneId);
        if (zone.IsLost)
        {
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