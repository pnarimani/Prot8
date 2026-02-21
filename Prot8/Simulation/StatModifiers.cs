namespace Prot8.Simulation;

public static class StatModifiers
{
    public static double ComputeGlobalProductionMultiplier(GameState state)
    {
        var moraleFactor = 0.75 + (state.Morale / 200.0);
        var unrestFactor = 1.0 - (state.Unrest / 220.0);
        var sicknessFactor = 1.0 - (state.Sickness / 250.0);

        var combined = moraleFactor * unrestFactor * sicknessFactor;
        if (combined < 0.25)
        {
            return 0.25;
        }

        if (combined > 1.3)
        {
            return 1.3;
        }

        return combined;
    }

    public static int ComputeSicknessFromEnvironment(GameState state)
    {
        var delta = 1;

        if (state.Unrest >= 50)
        {
            delta += 1;
        }

        if (state.Resources[Resources.ResourceKind.Fuel] == 0)
        {
            delta += 1;
        }

        return delta;
    }

    public static int ComputeUnrestProgression(GameState state)
    {
        var delta = 1;
        delta += state.Morale < 40 ? 2 : 0;
        delta += state.Morale < 25 ? 2 : 0;
        delta += state.Sickness > 50 ? 1 : 0;
        delta += state.CountLostZones();
        delta -= state.Population.Guards >= 15 ? 1 : 0;

        return delta < 0 ? 0 : delta;
    }

    public static int ComputeMoraleDrift(GameState state)
    {
        var delta = -1;
        delta -= state.Unrest > 60 ? 1 : 0;
        delta -= state.Sickness > 60 ? 1 : 0;
        delta -= state.CountLostZones();
        return delta;
    }
}