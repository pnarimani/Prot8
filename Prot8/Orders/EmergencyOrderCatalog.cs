using System.Collections.Generic;

namespace Prot8.Orders;

public static class EmergencyOrderCatalog
{
    private static readonly IReadOnlyList<IEmergencyOrder> AllOrders = new IEmergencyOrder[]
    {
        // Original orders
        new DivertSuppliesToRepairsOrder(),
        new SoupKitchensOrder(),
        new EmergencyWaterRationOrder(),
        new CrackdownPatrolsOrder(),
        new InspireThePeopleOrder(),
        new VoluntaryEvacuationOrder(),
        new ScavengeMedicineOrder(),
        new QuarantineDistrictOrder(),
        // Former decrees (converted to orders)
        new BurnSurplusOrder(),
        new DistributeFuelOrder(),
        new DoubleWaterRationsOrder(),
        new ForcedLaborOrder(),
        new RallyGuardsOrder(),
        // New orders
        new SewerCleanupOrder(),
        new SacrificeTheSickOrder(),
        new FortifyTheGateOrder(),
        new RationMedicineOrder(),
    };

    public static IReadOnlyList<IEmergencyOrder> GetAll() => AllOrders;

    public static IEmergencyOrder? Find(string orderId)
    {
        foreach (var order in AllOrders)
        {
            if (order.Id == orderId)
            {
                return order;
            }
        }

        return null;
    }
}
