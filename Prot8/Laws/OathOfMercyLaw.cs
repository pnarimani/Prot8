using Prot8.Simulation;

namespace Prot8.Laws;

public sealed class OathOfMercyLaw : ILaw
{
    private const int DailyMorale = 5;
    private const int DailySicknessReduction = 2;
    private const double ProductionPenalty = 0.90;

    public string Id => "oath_of_mercy";
    public string Name => "Oath of Mercy";

    public string GetTooltip(GameState state) =>
        $"+{DailyMorale} morale/day, -{DailySicknessReduction} sickness/day, -10% production. Requires Faith >= 4 and Tyranny <= 2.";

    public bool CanEnact(GameState state)
    {
        if (state.Flags.IronFist)
        {
            return false;
        }

        if (state.Flags.Faith < 4)
        {
            return false;
        }

        if (state.Flags.Tyranny > 2)
        {
            return false;
        }

        return true;
    }

    public void OnEnact(GameState state, ResolutionEntry entry)
    {
        state.Flags.Faith.Add(3);
        state.Flags.PeopleFirst.Set();
        entry.Write("An oath is sworn before the people: no life shall be taken by decree. The sick shall be tended, the hungry fed. Production slows, but hope endures.");
    }

    public void ApplyDaily(GameState state, ResolutionEntry entry)
    {
        state.AddMorale(DailyMorale, entry);
        state.AddSickness(-DailySicknessReduction, entry);
        state.DailyEffects.ProductionMultiplier.Apply("Oath of Mercy", ProductionPenalty);
        entry.Write("The oath holds. Healers tend the sick freely, and the people find solace in compassion.");
    }
}
