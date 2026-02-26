using Prot8.Constants;
using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Laws;

public sealed class CannibalismLaw : ILaw
{
    public string Id => "cannibalism";
    public string Name => "Cannibalism Law";

    public string GetTooltip(GameState state) =>
        $"Permanent. Food from deaths (1-10/day), -5 morale/day, +3 sickness/day, -3 unrest/day. " +
        $"On enact: Tyranny +3, Fear +2, +20 unrest, 5 worker desertions. " +
        $"15% daily guard desertion chance, 10% daily worker desertion chance. " +
        $"Requires food <= {GameBalance.CannibalismFoodThreshold} and food deficit.";

    public bool CanEnact(GameState state)
    {
        if (!GameBalance.EnableCannibalismLaw)
            return false;
        if (state.Flags.CannibalismEnacted)
            return false;
        if (state.Resources[ResourceKind.Food] > GameBalance.CannibalismFoodThreshold)
            return false;
        if (state.ConsecutiveFoodDeficitDays < 1)
            return false;
        return true;
    }

    public void OnEnact(GameState state, ResolutionEntry entry)
    {
        state.Flags.CannibalismEnacted.Set();
        state.Flags.Tyranny.Add(GameBalance.CannibalismTyrannyGain);
        state.Flags.FearLevel.Add(GameBalance.CannibalismFearGain);
        state.AddUnrest(GameBalance.CannibalismOnEnactUnrest, entry);

        var deserted = state.ApplyWorkerDesertion(GameBalance.CannibalismOnEnactDesertions);
        if (deserted > 0)
            entry.Write($"{deserted} workers fled in horror at the proclamation.");

        entry.Write("A terrible law is passed. The dead shall feed the living. No one speaks of it openly, but the smell of the cookfires has changed.");
    }

    public void ApplyDaily(GameState state, ResolutionEntry entry)
    {
        var recentDeaths = state.TotalDeaths - state.DeathsAtStartOfDay;
        var foodFromDeaths = Math.Clamp(Math.Max(1, recentDeaths * GameBalance.CannibalismFoodPerDeath), 1, GameBalance.CannibalismMaxFoodPerDay);
        state.AddResource(ResourceKind.Food, foodFromDeaths, entry);
        entry.Write($"The grim harvest yields {foodFromDeaths} food.");

        state.AddMorale(GameBalance.CannibalismDailyMorale, entry);
        state.AddSickness(GameBalance.CannibalismDailySickness, entry);
        state.AddUnrest(GameBalance.CannibalismDailyUnrest, entry);

        if (state.RollPercent() <= GameBalance.CannibalismGuardDesertionChance)
        {
            var guardDeserters = state.Random.Next(1, 3);
            var actualDeserters = Math.Min(guardDeserters, state.Population.Guards);
            if (actualDeserters > 0)
            {
                state.ApplyGuardDesertion(actualDeserters);
                state.TotalDesertions += actualDeserters;
                entry.Write($"{actualDeserters} guards deserted, unable to stomach the new order.");
            }
        }

        if (state.RollPercent() <= GameBalance.CannibalismWorkerDesertionChance)
        {
            var workerDeserters = state.Random.Next(2, 4);
            var deserted = state.ApplyWorkerDesertion(workerDeserters);
            if (deserted > 0)
                entry.Write($"{deserted} workers slipped away in the night.");
        }
    }
}
