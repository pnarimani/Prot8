using System.Collections.Generic;

namespace Prot8.Orders;

public static class EmergencyOrderCatalog
{
    private static readonly IReadOnlyList<IEmergencyOrder> _allOrders =
    [
        new DivertSuppliesToRepairsOrder(),
        new RallyGuardsOrder(),
        new CrackdownPatrolsOrder(),
        new VoluntaryEvacuationOrder(),
        new ScavengeMedicineOrder(),
        new QuarantineDistrictOrder(),
        new BurnSurplusOrder(),
        new ForcedLaborOrder(),
        new SacrificeTheSickOrder(),
        new FortifyTheGateOrder(),
        new RationMedicineOrder(),
        new PublicConfessionOrder(),
        new InspireThePeopleOrder(),
        new ReinforceTheWallsOrder(),
        new HoldAFeastOrder(),
        new DayOfRemembranceOrder(),
        new PublicTrialOrder(),
        new StorytellingNightOrder(),
        new DistributeLuxuriesOrder(),
    ];

    public static IReadOnlyList<IEmergencyOrder> GetAll() => _allOrders;

    public static IEmergencyOrder? Find(string orderId)
    {
        foreach (var order in _allOrders)
        {
            if (order.Id == orderId)
            {
                return order;
            }
        }

        return null;
    }
}
