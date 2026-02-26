using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Laws;

public sealed class GarrisonMandateLaw : ILaw
{
    private const int WorkersConverted = 3;
    private const int DailyFoodCost = 5;
    private const int MoraleHit = 5;
    private const int MilitiaTrainingInterval = 3;

    public string Id => "garrison_mandate";
    public string Name => "Garrison Mandate";

    private int _daysSinceLastMilitia;

    public string GetTooltip(GameState state) =>
        $"Convert {WorkersConverted} workers to guards on enact. +1 guard every {MilitiaTrainingInterval} days. -{DailyFoodCost} food/day, -{MoraleHit} morale on enact. Requires Fortification >= 4.";

    public bool CanEnact(GameState state)
    {
        if (state.Flags.PeopleFirst)
        {
            return false;
        }

        if (state.Flags.Fortification < 4)
        {
            return false;
        }

        return true;
    }

    public void OnEnact(GameState state, ResolutionEntry entry)
    {
        state.Flags.Fortification.Add(2);
        state.Flags.GarrisonState.Set();
        var converted = state.Population.ConvertHealthyToGuards(WorkersConverted);
        state.AddMorale(-MoraleHit, entry);
        _daysSinceLastMilitia = 0;
        entry.Write($"The garrison mandate is declared. {converted} workers take up arms. Every able body will serve the walls.");
    }

    public void ApplyDaily(GameState state, ResolutionEntry entry)
    {
        state.AddResource(ResourceKind.Food, -DailyFoodCost, entry);
        _daysSinceLastMilitia++;

        if (_daysSinceLastMilitia >= MilitiaTrainingInterval)
        {
            _daysSinceLastMilitia = 0;
            var converted = state.Population.ConvertHealthyToGuards(1);
            if (converted > 0)
            {
                entry.Write("Militia training complete. A new guard joins the garrison.");
            }
        }

        entry.Write("The garrison eats well. The workers less so.");
    }
}
