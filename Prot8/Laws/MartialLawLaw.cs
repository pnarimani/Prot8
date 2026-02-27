using Prot8.Constants;
using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Laws;

public sealed class MartialLawLaw : ILaw
{
    private const int UnrestCap = 60;
    private const int MoraleCap = 35;
    private const int UnrestThreshold = 75;
    private const int DailyDeaths = 2;
    private const int DailyFoodCost = 10;

    public string Id => "martial_law";
    public string Name => "Martial Law";

    public string GetTooltip(GameState state) => $"Unrest cannot exceed {UnrestCap}, morale capped at {MoraleCap}. {DailyDeaths} executions/day, -{DailyFoodCost} food/day. Requires unrest > {UnrestThreshold}. Incompatible with Curfew.";

    public bool CanEnact(GameState state)
    {
        if (state.Flags.Faith >= 5)
        {
            return false;
        }

        if (state.ActiveLawIds.Contains("curfew"))
        {
            return false;
        }

        if (state.Flags.Tyranny < 2)
        {
            return false;
        }

        if (state.Unrest > UnrestThreshold)
        {
            return true;
        }

        return false;
    }

    public void OnEnact(GameState state, ResolutionEntry entry)
    {
        state.Flags.Tyranny.Add(3);
        state.Flags.FearLevel.Add(1);
        state.Flags.MartialState.Set();
        if (GameBalance.EnableHumanityScore) state.Flags.Humanity.Add(-8);
        entry.Write("The garrison takes control. Soldiers patrol every street. Dissent will be answered with steel.");
    }

    public void ApplyDaily(GameState state, ResolutionEntry entry)
    {
        if (state.Unrest > UnrestCap)
        {
            var reduction = state.Unrest - UnrestCap;
            state.AddUnrest(-reduction, entry);
            entry.Write("Martial law suppresses unrest by force.");
        }

        if (state.Morale > MoraleCap)
        {
            var reduction = state.Morale - MoraleCap;
            state.AddMorale(-reduction, entry);
            entry.Write("Hope is a luxury the garrison cannot afford.");
        }

        state.ApplyDeath(DailyDeaths, entry);
        state.AddResource(ResourceKind.Food, -DailyFoodCost, entry);
        entry.Write("The price of order: blood and bread.");
    }
}
