namespace Prot8.Simulation;

public static class StatModifiers
{
    public static double ComputeGlobalProductionMultiplier(GameState state)
    {
        var moraleFactor = 0.75 + (state.Morale / 200.0);
        var unrestFactor = 1.0 - (state.Unrest / 200.0);
        var sicknessFactor = 1.0 - (state.Sickness / 200.0);

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

        if (state.Unrest >= 75)
        {
            delta += 1;
        }

        if (state.Resources[Resources.ResourceKind.Fuel] == 0)
        {
            delta += 2;
        }

        if (state.PlagueRatsActive)
        {
            delta += 3;
        }

        return delta;
    }

    public static int ComputeUnrestProgression(GameState state)
    {
        var delta = 1;
        delta += state.Morale < 40 ? 1 : 0;
        delta += state.Morale < 25 ? 3 : 0;
        delta += state.Sickness > 50 ? 1 : 0;
        delta += state.Sickness > 70 ? 1 : 0;
        delta += state.CountLostZones();
        delta -= state.Population.Guards / 5;

        return delta < 0 ? 0 : delta;
    }

    public static int ComputeMoraleDrift(GameState state)
    {
        var delta = -1;
        delta -= state.Unrest > 50 ? 1 : 0;
        delta -= state.Sickness > 50 ? 1 : 0;
        delta -= state.CountLostZones();
        delta -= state.Day > 25 ? 1 : 0;
        return delta;
    }
}
