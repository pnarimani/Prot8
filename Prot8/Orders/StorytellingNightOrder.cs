using Prot8.Constants;
using Prot8.Simulation;

namespace Prot8.Orders;

public sealed class StorytellingNightOrder : IEmergencyOrder
{
    public string Id => "storytelling_night";
    public string Name => "Storytelling Night";
    public int CooldownDays => GameBalance.StorytellingNightCooldown;

    public string GetTooltip(GameState state) =>
        $"No cost. +{GameBalance.StorytellingNightMoraleGain} morale. " +
        $"Requires morale between {GameBalance.StorytellingNightMoraleMin}-{GameBalance.StorytellingNightMoraleMax}.";

    public bool CanIssue(GameState state)
    {
        if (!GameBalance.EnableMoraleOrders)
            return false;
        if (state.Morale < GameBalance.StorytellingNightMoraleMin || state.Morale > GameBalance.StorytellingNightMoraleMax)
            return false;
        return true;
    }

    public void Apply(GameState state, ResolutionEntry entry)
    {
        state.AddMorale(GameBalance.StorytellingNightMoraleGain, entry);
        entry.Write("As darkness falls, elders gather the people around flickering fires. Stories of better times remind them why they endure.");
    }
}
