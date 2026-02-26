using System.Collections.Generic;

namespace Prot8.Events;

public static class EventCatalog
{
    private static readonly IReadOnlyList<ITriggeredEvent> AllEvents = new ITriggeredEvent[]
    {
        new OpeningBombardmentEvent(),
        new SupplyCartsInterceptedEvent(),
        new SmugglerAtTheGateEvent(),
        new WellContaminationScareEvent(),
        new MilitiaVolunteersEvent(),
        new HungerRiotEvent(),
        new FeverOutbreakEvent(),
        new DesertionWaveEvent(),
        new WallBreachAttemptEvent(),
        new FireInArtisanQuarterEvent(),
        new SiegeBombardmentEvent(),
        new DespairEvent(),
        new RefugeesAtTheGatesEvent(),
        new EnemySappersEvent(),
        new PlagueRatsEvent(),
        new TaintedWellEvent(),
        new EnemyUltimatumEvent(),
        new FinalAssaultEvent(),
        new BetrayalFromWithinEvent(),
        new CouncilRevoltEvent(),
        new TotalCollapseEvent(),
        new BlackMarketTradingEvent(),
        new SpySellingIntelEvent(),
        new IntelSiegeWarningEvent(),
        new DissidentsDiscoveredEvent(),
        new ChildrensPleaEvent(),
        new TyrantsReckoningEvent(),
        new SiegeEngineersArriveEvent(),
        new CrisisOfFaithEvent(),
        new NarrativeSiegeBeatEvent("narrative_messenger", "Enemy Messenger",
            1, "A messenger arrives under white flag. \"Surrender the city, and your people will be spared.\" You send him back."),
        new NarrativeSiegeBeatEvent("narrative_towers", "Siege Towers Spotted",
            7, "Scouts report the enemy has built siege towers. The bombardment will intensify."),
        new NarrativeSiegeBeatEvent("narrative_letter", "Enemy Commander's Letter",
            15, "A letter from the enemy commander: \"Your walls weaken. Your people starve. How many more must die for your pride?\""),
        new NarrativeSiegeBeatEvent("narrative_burning_farms", "Burning Farms",
            25, "Smoke rises beyond the walls. The enemy is burning the farms they captured. There will be nothing to reclaim."),
        new NarrativeSiegeBeatEvent("narrative_horns", "Distant Horns",
            38, "Horns in the distance. Relief? Or the final assault? You cannot tell."),
    };

    public static IReadOnlyList<ITriggeredEvent> GetAll() => AllEvents;
}
