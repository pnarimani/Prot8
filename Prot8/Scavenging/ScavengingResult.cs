using Prot8.Resources;

namespace Prot8.Scavenging;

public sealed class ScavengingResult
{
    public string LocationName { get; init; } = "";
    public List<(ResourceKind Kind, int Amount)> ResourcesGained { get; init; } = [];
    public int Deaths { get; init; }
    public int Wounded { get; init; }
    public int WorkersReturned { get; init; }
    public bool LocationDepleted { get; init; }
    public string Narrative { get; init; } = "";
    public bool IntelGained { get; init; }
}
