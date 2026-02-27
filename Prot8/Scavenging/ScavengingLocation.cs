namespace Prot8.Scavenging;

public sealed class ScavengingLocation
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required DangerLevel Danger { get; init; }
    public int MinWorkers { get; init; } = 2;
    public int MaxWorkers { get; init; } = 4;
    public required int MaxVisits { get; init; }
    public int VisitsRemaining { get; set; }
    public required List<ScavengingReward> PossibleRewards { get; init; }
    public required int CasualtyChancePercent { get; init; }
    public int MaxCasualties { get; init; } = 1;
    public bool ProvidesIntel { get; init; }
}
