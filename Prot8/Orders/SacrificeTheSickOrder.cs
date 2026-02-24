using Prot8.Simulation;

namespace Prot8.Orders;

public sealed class SacrificeTheSickOrder : IEmergencyOrder
{
    private const int SickKilled = 2;
    private const int SicknessReduction = 5;
    private const int UnrestGain = 8;
    private const int SickThreshold = 3;

    public string Id => "sacrifice_sick";
    public string Name => "Sacrifice the Sick";
    public int CooldownDays => 3;

    public string GetTooltip(GameState state) =>
        $"Kill {SickKilled} sick workers, -{SicknessReduction} sickness, +{UnrestGain} unrest. Requires sick > {SickThreshold}.";

    public bool CanIssue(GameState state, out string reason)
    {
        if (state.Population.SickWorkers <= SickThreshold)
        {
            reason = $"Requires more than {SickThreshold} sick workers.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    public void Apply(GameState state, ResolutionEntry entry)
    {
        var killed = Math.Min(SickKilled, state.Population.SickWorkers);
        state.Population.RemoveSickWorkers(killed);
        state.TotalDeaths += killed;
        entry.Write($"{Name}: {killed} sick workers put down.");
        state.AddSickness(-SicknessReduction, entry);
        state.AddUnrest(UnrestGain, entry);
    }
}
