using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Orders;

public sealed class InspireThePeopleOrder : IEmergencyOrder
{
    private const int MoraleGain = 15;
    private const int FoodCost = 5;
    private const int WaterCost = 5;

    public string Id => "inspire_people";
    public string Name => "Inspire the People";
    public int CooldownDays => 4;

    public string GetTooltip(GameState state) =>
        $"+{MoraleGain} morale, -{FoodCost} food, -{WaterCost} water (feast). Requires Faith >= 2.";

    public bool CanIssue(GameState state)
    {
        if (state.Flags.Faith < 2)
        {
            return false;
        }

        if (!state.Resources.Has(ResourceKind.Food, FoodCost))
        {
            return false;
        }

        if (!state.Resources.Has(ResourceKind.Water, WaterCost))
        {
            return false;
        }

        return true;
    }

    public void Apply(GameState state, ResolutionEntry entry)
    {
        state.Flags.Faith.Add(1, lifetimeDays: 3);
        state.AddResource(ResourceKind.Food, -FoodCost, entry);
        state.AddResource(ResourceKind.Water, -WaterCost, entry);
        state.AddMorale(MoraleGain, entry);
        entry.Write("A feast is held in the town square. Songs are sung, stories shared. For one evening, the siege feels distant. Hope is renewed.");
    }
}
