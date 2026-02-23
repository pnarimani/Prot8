using System.Collections.Generic;

namespace Prot8.Decrees;

public static class DecreeCatalog
{
    private static readonly IReadOnlyList<IDecree> AllDecrees = new IDecree[]
    {
        new DoubleWaterRationsDecree(),
        new ForcedLaborDetailDecree(),
        new DistributeFuelReservesDecree(),
        new RallyTheGuardsDecree(),
        new BurnSurplusForWarmthDecree(),
    };

    public static IReadOnlyList<IDecree> GetAll() => AllDecrees;

    public static IDecree? Find(string decreeId)
    {
        foreach (var decree in AllDecrees)
        {
            if (decree.Id == decreeId)
            {
                return decree;
            }
        }

        return null;
    }
}
