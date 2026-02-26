using Prot8.Constants;
using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Diplomacy;

public sealed class OfferTributeAction : IDiplomaticAction
{
    public string Id => "offer_tribute";
    public string Name => "Offer Tribute";
    public bool CanDeactivate => true;

    public string GetTooltip(GameState state) =>
        $"Daily: -{GameBalance.TributeFoodCost} food, -{GameBalance.TributeWaterCost} water. " +
        $"Siege escalation paused, -{-GameBalance.TributeDailyMorale} morale/day.";

    public bool CanActivate(GameState state) => true;

    public void OnActivate(GameState state, ResolutionEntry entry)
    {
        entry.Write("Carts of food and water are sent to the enemy camp. The siege pauses... at a terrible cost to our stores.");
    }

    public void ApplyDaily(GameState state, ResolutionEntry entry)
    {
        state.AddResource(ResourceKind.Food, -GameBalance.TributeFoodCost, entry);
        state.AddResource(ResourceKind.Water, -GameBalance.TributeWaterCost, entry);
        state.AddMorale(GameBalance.TributeDailyMorale, entry);

        state.SiegeEscalationDelayDays = Math.Max(state.SiegeEscalationDelayDays, 1);
    }
}
