using Prot8.Simulation;

namespace Prot8.Laws;

public sealed class ConscriptElderlyLaw : ILaw
{
    private const int MoraleHit = 20;
    private const int UnrestHit = 10;
    private const int DailyDeaths = 1;
    private const int MinimumDay = 8;

    public string Id => "conscript_elderly";
    public string Name => "Conscript the Elderly";
    public string GetTooltip(GameState state) => $"Convert all elderly ({state.Population.Elderly}) to workers. -{MoraleHit} morale, +{UnrestHit} unrest. {DailyDeaths} death/day from exhaustion. Day {MinimumDay}+.";

    public bool CanEnact(GameState state, out string reason)
    {
        if (state.Day < MinimumDay)
        {
            reason = $"Available from Day {MinimumDay}.";
            return false;
        }

        if (state.Population.Elderly <= 0)
        {
            reason = "No elderly remain.";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    public void OnEnact(GameState state, DayResolutionReport report)
    {
        var converted = state.Population.Elderly;
        state.Population.Elderly = 0;
        state.Population.HealthyWorkers += converted;
        report.Add(ReasonTags.LawEnact, $"{Name}: {converted} elderly forced into labour.");
        StateChangeApplier.AddMorale(state, -MoraleHit, report, ReasonTags.LawEnact, Name);
        StateChangeApplier.AddUnrest(state, UnrestHit, report, ReasonTags.LawEnact, Name);
    }

    public void ApplyDaily(GameState state, DayResolutionReport report)
    {
        StateChangeApplier.ApplyDeaths(state, DailyDeaths, report, ReasonTags.LawPassive, $"{Name} toll");
    }
}
