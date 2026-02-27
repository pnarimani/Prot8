using Prot8.Constants;
using Prot8.Simulation;

namespace Prot8.Orders;

public sealed class DayOfRemembranceOrder : IEmergencyOrder
{
    public string Id => "day_of_remembrance";
    public string Name => "Day of Remembrance";
    public int CooldownDays => GameBalance.DayOfRemembranceCooldown;

    public string GetTooltip(GameState state) =>
        $"Halts all production for the day. +{GameBalance.DayOfRemembranceMoraleGain} morale, " +
        $"-{-GameBalance.DayOfRemembranceUnrest} unrest, -{-GameBalance.DayOfRemembranceSickness} sickness. " +
        $"Faith +{GameBalance.DayOfRemembranceFaithGain}. Requires morale < {GameBalance.DayOfRemembranceMoraleGate}.";

    public bool CanIssue(GameState state)
    {
        if (!GameBalance.EnableMoraleOrders)
            return false;
        if (state.Morale >= GameBalance.DayOfRemembranceMoraleGate)
            return false;
        return true;
    }

    public void Apply(GameState state, ResolutionEntry entry)
    {
        state.DailyEffects.ProductionMultiplier.Apply("Day of Remembrance", 0.0);
        state.AddMorale(GameBalance.DayOfRemembranceMoraleGain, entry);
        state.AddUnrest(GameBalance.DayOfRemembranceUnrest, entry);
        state.AddSickness(GameBalance.DayOfRemembranceSickness, entry);
        state.Flags.Faith.Add(GameBalance.DayOfRemembranceFaithGain);
        if (GameBalance.EnableHumanityScore) state.Flags.Humanity.Add(3);
        entry.Write("All work ceases. Names of the fallen are read aloud. The city mourns together, and finds strength in shared grief.");
    }
}
