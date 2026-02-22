using Prot8.Constants;
using Prot8.Jobs;
using Prot8.Laws;
using Prot8.Missions;
using Prot8.Orders;
using Prot8.Simulation;
using Prot8.Zones;

namespace Prot8.Cli;

public static class ActionAvailability
{
    public static IReadOnlyList<JobType> GetJobTypes() => Enum.GetValues<JobType>();

    public static IReadOnlyList<ILaw> GetAvailableLaws(GameState state)
    {
        var available = new List<ILaw>();

        var cooldownActive = state.LastLawDay != int.MinValue
                             && state.Day - state.LastLawDay < GameBalance.LawCooldownDays;
        if (cooldownActive)
            return available;

        foreach (var law in LawCatalog.GetAll())
        {
            if (state.ActiveLawIds.Contains(law.Id))
                continue;

            if (law.CanEnact(state, out _))
                available.Add(law);
        }

        return available;
    }

    public static IReadOnlyList<IMissionDefinition> GetAvailableMissions(GameState state)
    {
        var available = new List<IMissionDefinition>();

        foreach (var mission in MissionCatalog.GetAll())
        {
            if (mission.CanStart(state, out _))
                available.Add(mission);
        }

        return available;
    }

    public static IReadOnlyList<IEmergencyOrder> GetAvailableOrders(GameState state)
    {
        var available = new List<IEmergencyOrder>();

        foreach (var order in EmergencyOrderCatalog.GetAll())
        {
            if (order.CanIssue(state, out _))
                available.Add(order);
        }

        return available;
    }
}