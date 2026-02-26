using Prot8.Constants;
using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Events;

public sealed class SpySellingIntelEvent : IRespondableEvent
{
    public string Id => "spy_selling_intel";
    public string Name => "Spy Selling Intel";

    public string Description =>
        "A hooded figure claims to be a deserter from the enemy camp. He offers military intelligence — troop movements, siege plans — in exchange for supplies.";

    public bool ShouldTrigger(GameState state)
    {
        if (!GameBalance.EnableSpyIntelEvent)
            return false;

        if (state.Day < GameBalance.SpyIntelMinDay)
            return false;

        return state.RollPercent() <= GameBalance.SpyIntelTriggerChance;
    }

    public void ResolveNow(GameState state, ResolutionEntry entry)
    {
        ResolveWithResponse("refuse", state, entry);
    }

    public IReadOnlyList<EventResponse> GetResponses(GameState state)
    {
        return
        [
            new EventResponse("accept",
                $"Buy Intel (-{GameBalance.SpyIntelMaterialsCost} Materials, -{GameBalance.SpyIntelFoodCost} Food → Intel buff {GameBalance.IntelBuffDurationDays} days)"),
            new EventResponse("refuse", "Turn him away"),
        ];
    }

    public void ResolveWithResponse(string responseId, GameState state, ResolutionEntry entry)
    {
        switch (responseId)
        {
            case "accept":
                if (!state.Resources.Has(ResourceKind.Materials, GameBalance.SpyIntelMaterialsCost) ||
                    !state.Resources.Has(ResourceKind.Food, GameBalance.SpyIntelFoodCost))
                {
                    entry.Write("You lack the supplies to pay the spy. He slips away into the night.");
                    return;
                }

                state.Resources.Consume(ResourceKind.Materials, GameBalance.SpyIntelMaterialsCost);
                state.Resources.Consume(ResourceKind.Food, GameBalance.SpyIntelFoodCost);
                state.IntelBuffDaysRemaining = GameBalance.IntelBuffDurationDays;
                state.IntelWarningPending = true;
                entry.Write(
                    $"The spy's information is detailed and credible. Intel buff active for {GameBalance.IntelBuffDurationDays} days. Missions gain +{GameBalance.IntelMissionSuccessBonus * 100:F0}% success.");
                break;

            default: // refuse
                entry.Write("You refuse the spy's offer. He disappears without a trace.");
                break;
        }
    }
}
