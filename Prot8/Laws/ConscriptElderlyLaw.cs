using Prot8.Simulation;

namespace Prot8.Laws;

public sealed class ConscriptElderlyLaw : ILaw
{
    const int MoraleHit = 20;
    const int UnrestHit = 10;
    const int DailyDeaths = 1;
    const int MinimumDay = 8;

    public string Id => "conscript_elderly";
    public string Name => "Conscript the Elderly";

    int _converted;

    public string GetTooltip(GameState state)
    {
        return
            $"Convert all elderly ({state.Population.Elderly}) to workers. -{MoraleHit} morale, +{UnrestHit} unrest. {DailyDeaths} death/day from exhaustion. Day {MinimumDay}+.";
    }

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

    public void OnEnact(GameState state, ResolutionEntry entry)
    {
        _converted = state.Population.Elderly;
        state.Population.Elderly = 0;
        state.Population.HealthyWorkers += _converted;
        entry.Write(
            $"The elderly are sent to the forges and walls. {_converted} former elders take up tools, their aged hands ill-suited for labor.");
        state.AddMorale(-MoraleHit, entry);
        state.AddUnrest(UnrestHit, entry);
    }

    public void ApplyDaily(GameState state, ResolutionEntry entry)
    {
        if (_converted <= 0)
        {
            return;
        }
        
        if (state.RollPercent() > 30)
        {
            return;
        }

        entry.Write("An elder collapses at the work site. The others keep digging.");
        state.ApplyDeath(DailyDeaths, entry);
        _converted--;
    }
}