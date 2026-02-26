using Prot8.Simulation;

namespace Prot8.Orders;

public sealed class VoluntaryEvacuationOrder : IEmergencyOrder
{
    public string Id => "voluntary_evacuation";
    public string Name => "Voluntary Evacuation";
    public int CooldownDays => 0;

    public string GetTooltip(GameState state) => "Evacuate the active perimeter zone if eligible (irreversible).";

    public bool CanIssue(GameState state, out string reason)
    {
        if (state.Flags.MartialState)
        {
            reason = "Martial authority forbids voluntary retreat.";
            return false;
        }

        return ZoneRules.CanEvacuate(state, state.ActivePerimeterZone.Id, out reason);
    }

    public void Apply(GameState state, ResolutionEntry entry)
    {
        state.Flags.Faith.Add(1);
        state.Flags.PeopleFirst.Set();
        var zoneName = state.ActivePerimeterZone.Name;
        entry.Write($"The {zoneName} is evacuated. Families are relocated deeper into the city. The perimeter shrinks, are but the people safe.");
        state.LoseZone(state.ActivePerimeterZone.Id, true, entry);
    }
}