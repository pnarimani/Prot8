using Prot8.Constants;
using Prot8.Simulation;

namespace Prot8.Orders;

public sealed class SacrificeTheSickOrder : IEmergencyOrder
{
    private const int SickKilled = 3;
    private const int SicknessReduction = 8;
    private const int UnrestGain = 12;
    private const int MoraleHit = 10;
    private const int SickThreshold = 5;

    public string Id => "sacrifice_sick";
    public string Name => "Sacrifice the Sick";
    public int CooldownDays => 3;

    public string GetTooltip(GameState state) =>
        $"Kill {SickKilled} sick workers, -{SicknessReduction} sickness, +{UnrestGain} unrest, -{MoraleHit} morale. Requires sick > {SickThreshold}.";

    public bool CanIssue(GameState state)
    {
        if (state.Flags.PeopleFirst)
        {
            return false;
        }

        if (state.Flags.Tyranny < 3)
        {
            return false;
        }

        if (state.Population.SickWorkers <= SickThreshold)
        {
            return false;
        }

        return true;
    }

    public void Apply(GameState state, ResolutionEntry entry)
    {
        state.Flags.Tyranny.Add(2);
        state.Flags.MercyDenied.Set();
        if (GameBalance.EnableHumanityScore) state.Flags.Humanity.Add(-12);
        var killed = Math.Min(SickKilled, state.Population.SickWorkers);
        state.Population.RemoveSickWorkers(killed);
        state.TotalDeaths += killed;
        entry.Write($"{killed} sick are dragged from their beds and put to the sword. The pyres burn through the night. It is mercy, the garrison says — but no one believes it.");
        state.AddSickness(-SicknessReduction, entry);
        state.AddUnrest(UnrestGain, entry);
        state.AddMorale(-MoraleHit, entry);
    }
}
