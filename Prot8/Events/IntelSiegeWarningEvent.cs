using Prot8.Constants;
using Prot8.Simulation;

namespace Prot8.Events;

public sealed class IntelSiegeWarningEvent : IRespondableEvent
{
    public string Id => "intel_siege_warning";
    public string Name => "Intel: Siege Warning";

    public string Description =>
        "Your intelligence reveals an imminent enemy assault. The enemy is massing forces for a major strike. You have time to prepare — but every choice carries risk.";

    public bool ShouldTrigger(GameState state)
    {
        if (!GameBalance.EnableSpyIntelEvent)
            return false;

        if (!state.IntelWarningPending)
            return false;

        state.IntelWarningPending = false;
        return true;
    }

    public void ResolveNow(GameState state, ResolutionEntry entry)
    {
        ResolveWithResponse("brace", state, entry);
    }

    public IReadOnlyList<EventResponse> GetResponses(GameState state)
    {
        return
        [
            new EventResponse("intercept",
                $"Intercept: Send {GameBalance.IntelInterceptGuardCost} guards to ambush (risk {GameBalance.IntelInterceptGuardDeathRisk} deaths, reduce siege damage for {GameBalance.IntelInterceptDurationDays} days)"),
            new EventResponse("brace",
                $"Brace: Reinforce perimeter (+{GameBalance.IntelBraceIntegrityBonus} integrity)"),
        ];
    }

    public void ResolveWithResponse(string responseId, GameState state, ResolutionEntry entry)
    {
        switch (responseId)
        {
            case "intercept":
                if (state.Population.Guards < GameBalance.IntelInterceptGuardCost)
                {
                    entry.Write("Not enough guards for the interception. You brace instead.");
                    ApplyBrace(state, entry);
                    return;
                }

                state.ApplyGuardDeath(GameBalance.IntelInterceptGuardDeathRisk, entry);
                state.SiegeDamageMultiplier = GameBalance.IntelInterceptSiegeDamageReduction;
                state.SiegeDamageReductionDaysRemaining = GameBalance.IntelInterceptDurationDays;
                entry.Write(
                    $"Guards ambushed the enemy vanguard. Siege damage reduced to {GameBalance.IntelInterceptSiegeDamageReduction * 100:F0}% for {GameBalance.IntelInterceptDurationDays} days.");
                break;

            default: // brace
                ApplyBrace(state, entry);
                break;
        }
    }

    static void ApplyBrace(GameState state, ResolutionEntry entry)
    {
        var perimeter = state.ActivePerimeterZone;
        perimeter.Integrity = Math.Min(100, perimeter.Integrity + GameBalance.IntelBraceIntegrityBonus);
        entry.Write($"Walls reinforced. +{GameBalance.IntelBraceIntegrityBonus} integrity to {perimeter.Name}.");
    }
}
