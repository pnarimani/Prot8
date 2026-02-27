namespace Prot8.Characters;

public sealed class NamedCharacter
{
    public required string Name { get; init; }
    public required string Backstory { get; init; }
    public required CharacterTrait Trait { get; init; }
    public bool IsAlive { get; set; } = true;
    public bool HasDeserted { get; set; }
}
