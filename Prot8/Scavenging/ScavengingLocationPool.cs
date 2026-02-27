using Prot8.Constants;
using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Scavenging;

public static class ScavengingLocationPool
{
    static readonly List<Func<int, ScavengingLocation>> Templates =
    [
        id => new ScavengingLocation
        {
            Id = id, Name = "Abandoned Church", Description = "A ruined chapel. Medicine may remain in the crypt.",
            Danger = DangerLevel.Low, MaxVisits = 2, VisitsRemaining = 2, CasualtyChancePercent = GameBalance.NightPhaseDangerLowCasualty,
            PossibleRewards = [new(ResourceKind.Medicine, 4, 8)],
        },
        id => new ScavengingLocation
        {
            Id = id, Name = "Ruined Granary", Description = "The enemy looted most of it, but sacks of grain remain in the rubble.",
            Danger = DangerLevel.Medium, MaxVisits = 3, VisitsRemaining = 3, CasualtyChancePercent = GameBalance.NightPhaseDangerMediumCasualty,
            PossibleRewards = [new(ResourceKind.Food, 12, 25)],
        },
        id => new ScavengingLocation
        {
            Id = id, Name = "Enemy Supply Camp", Description = "A lightly guarded forward camp. Rich pickings if you survive.",
            Danger = DangerLevel.High, MaxVisits = 1, VisitsRemaining = 1, CasualtyChancePercent = GameBalance.NightPhaseDangerHighCasualty,
            MaxCasualties = 2,
            PossibleRewards = [new(ResourceKind.Materials, 15, 30)],
        },
        id => new ScavengingLocation
        {
            Id = id, Name = "Old Well", Description = "A well outside the walls. Water is precious.",
            Danger = DangerLevel.Low, MaxVisits = 3, VisitsRemaining = 3, CasualtyChancePercent = GameBalance.NightPhaseDangerLowCasualty,
            PossibleRewards = [new(ResourceKind.Water, 10, 20)],
        },
        id => new ScavengingLocation
        {
            Id = id, Name = "Merchant Caravan Wreck", Description = "An overturned merchant wagon. Mixed goods spill across the road.",
            Danger = DangerLevel.Medium, MaxVisits = 2, VisitsRemaining = 2, CasualtyChancePercent = GameBalance.NightPhaseDangerMediumCasualty,
            PossibleRewards = [new(ResourceKind.Food, 5, 10), new(ResourceKind.Materials, 5, 10), new(ResourceKind.Medicine, 2, 5)],
        },
        id => new ScavengingLocation
        {
            Id = id, Name = "Collapsed Mine", Description = "A dangerous mine shaft. Fuel and materials lie within.",
            Danger = DangerLevel.High, MaxVisits = 1, VisitsRemaining = 1, CasualtyChancePercent = GameBalance.NightPhaseDangerHighCasualty,
            MaxCasualties = 2,
            PossibleRewards = [new(ResourceKind.Fuel, 10, 18), new(ResourceKind.Materials, 8, 15)],
        },
        id => new ScavengingLocation
        {
            Id = id, Name = "Field Hospital Ruins", Description = "The remains of an army hospital. Medicine and food may still be salvageable.",
            Danger = DangerLevel.Medium, MaxVisits = 2, VisitsRemaining = 2, CasualtyChancePercent = GameBalance.NightPhaseDangerMediumCasualty,
            PossibleRewards = [new(ResourceKind.Medicine, 4, 10), new(ResourceKind.Food, 5, 12)],
        },
        id => new ScavengingLocation
        {
            Id = id, Name = "Watchtower", Description = "An abandoned watchtower. Intel on enemy movements awaits.",
            Danger = DangerLevel.High, MaxVisits = 1, VisitsRemaining = 1, CasualtyChancePercent = GameBalance.NightPhaseDangerHighCasualty,
            ProvidesIntel = true,
            PossibleRewards = [new(ResourceKind.Materials, 3, 6)],
        },
    ];

    public static List<ScavengingLocation> GenerateNightLocations(GameState state, int count = 4)
    {
        var available = new List<int>();
        for (var i = 0; i < Templates.Count; i++)
            available.Add(i);

        // Shuffle
        for (var i = available.Count - 1; i > 0; i--)
        {
            var j = state.Random.Next(0, i + 1);
            (available[i], available[j]) = (available[j], available[i]);
        }

        var pick = Math.Min(count, available.Count);
        var result = new List<ScavengingLocation>();
        for (var i = 0; i < pick; i++)
        {
            result.Add(Templates[available[i]](i + 1));
        }

        return result;
    }
}
