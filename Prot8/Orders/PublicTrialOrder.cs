using Prot8.Constants;
using Prot8.Simulation;

namespace Prot8.Orders;

public sealed class PublicTrialOrder : IEmergencyOrder
{
    public string Id => "public_trial";
    public string Name => "Public Trial";
    public int CooldownDays => GameBalance.PublicTrialCooldown;

    public string GetTooltip(GameState state)
    {
        var tyrannyPath = state.Flags.Tyranny > state.Flags.Faith;
        if (tyrannyPath)
            return $"2 deaths. -{-GameBalance.PublicTrialTyrannyUnrest} unrest, {GameBalance.PublicTrialTyrannyMorale} morale, Tyranny +1. " +
                   $"Requires Tyranny >= 2 or Faith >= 2.";
        return $"2 deaths. +{GameBalance.PublicTrialFaithMorale} morale, {GameBalance.PublicTrialFaithUnrest} unrest, Faith +1. " +
               $"Requires Tyranny >= 2 or Faith >= 2.";
    }

    public bool CanIssue(GameState state)
    {
        if (!GameBalance.EnableMoraleOrders)
            return false;
        if (state.Flags.Tyranny < 2 && state.Flags.Faith < 2)
            return false;
        return true;
    }

    public void Apply(GameState state, ResolutionEntry entry)
    {
        state.ApplyDeath(GameBalance.PublicTrialDeaths, entry);

        if (state.Flags.Tyranny > state.Flags.Faith)
        {
            state.AddUnrest(GameBalance.PublicTrialTyrannyUnrest, entry);
            state.AddMorale(GameBalance.PublicTrialTyrannyMorale, entry);
            state.Flags.Tyranny.Add(1);
            entry.Write("The accused are dragged before the crowd. Justice is swift, brutal, and public. Fear holds the city in line.");
        }
        else
        {
            state.AddMorale(GameBalance.PublicTrialFaithMorale, entry);
            state.AddUnrest(GameBalance.PublicTrialFaithUnrest, entry);
            state.Flags.Faith.Add(1);
            entry.Write("A solemn tribunal convenes. The guilty confess before the faithful. The community is cleansed through righteous judgment.");
        }
    }
}
