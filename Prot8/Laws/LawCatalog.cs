using System.Collections.Generic;

namespace Prot8.Laws;

public static class LawCatalog
{
    private static readonly IReadOnlyList<ILaw> AllLaws = new ILaw[]
    {
        new StrictRationsLaw(),
        new WaterRationingLaw(),
        new ExtendedShiftsLaw(),
        new MandatoryGuardServiceLaw(),
        new EmergencySheltersLaw(),
        new PublicExecutionsLaw(),
        new FaithProcessionsLaw(),
        new FoodConfiscationLaw(),
        new MedicalTriageLaw(),
        new CurfewLaw(),
        new AbandonOuterRingLaw(),
        new MartialLawLaw(),
        new ConscriptElderlyLaw(),
        new BurnTheDeadLaw(),
        new ShadowCouncilLaw(),
        new PurgeTheDisloyalLaw(),
        new OathOfMercyLaw(),
        new CollectiveFarmsLaw(),
        new GarrisonMandateLaw(),
        new ScorchedEarthDoctrineLaw(),
    };

    public static IReadOnlyList<ILaw> GetAll() => AllLaws;

    public static ILaw? Find(string lawId)
    {
        foreach (var law in AllLaws)
        {
            if (law.Id == lawId)
            {
                return law;
            }
        }

        return null;
    }
}
