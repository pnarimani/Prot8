using Prot8.Simulation;

namespace Prot8.Laws;

public abstract class LawBase : ILaw
{
    protected LawBase(string id, string name, string summary)
    {
        Id = id;
        Name = name;
        Summary = summary;
    }

    public string Id { get; }

    public string Name { get; }

    public string Summary { get; }

    public virtual string GetDynamicTooltip(GameState state) => Summary;

    public abstract bool CanEnact(GameState state, out string reason);

    public virtual void OnEnact(GameState state, DayResolutionReport report)
    {
    }

    public virtual void ApplyDaily(GameState state, DayResolutionReport report)
    {
    }
}