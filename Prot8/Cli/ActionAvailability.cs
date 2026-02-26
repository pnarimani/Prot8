using Prot8.Constants;
using Prot8.Laws;
using Prot8.Missions;
using Prot8.Orders;
using Prot8.Simulation;
using Prot8.Zones;

namespace Prot8.Cli;

public static class ActionAvailability
{
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

            if (law.CanEnact(state))
                available.Add(law);
        }

        return available;
    }

    public static IReadOnlyList<IMissionDefinition> GetAvailableMissions(GameState state)
    {
        var available = new List<IMissionDefinition>();

        foreach (var mission in MissionCatalog.GetAll())
        {
            if (state.MissionCooldowns.TryGetValue(mission.Id, out var lastDay))
            {
                if (state.Day - lastDay < GameBalance.MissionCooldownDays)
                    continue;
            }

            if (mission.CanStart(state))
                available.Add(mission);
        }

        return available;
    }

    public static IReadOnlyList<IEmergencyOrder> GetAvailableOrders(GameState state)
    {
        var available = new List<IEmergencyOrder>();

        foreach (var order in EmergencyOrderCatalog.GetAll())
        {
            if (state.OrderCooldowns.TryGetValue(order.Id, out var lastDay)
                && state.Day - lastDay < order.CooldownDays)
                continue;

            if (order.CanIssue(state))
                available.Add(order);
        }

        return available;
    }
}
