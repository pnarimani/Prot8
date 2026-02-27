using Prot8.Constants;
using Prot8.Resources;
using Prot8.Simulation;
using Prot8.Zones;

namespace Prot8.Events;

public sealed class SignalFireEvent : IRespondableEvent
{
    public string Id => "signal_fire";
    public string Name => "Light the Signal Fires?";
    public string Description => "The watchtower is intact and the keep still stands. If relief is coming, a signal fire could draw them closer — but it will also draw enemy attention.";

    public bool ShouldTrigger(GameState state)
    {
        if (!GameBalance.EnableReliefArmy)
            return false;

        if (state.SignalFireLit)
            return false;

        if (state.Day < 25)
            return false;

        var keep = state.GetZone(ZoneId.Keep);
        if (keep.IsLost || keep.Integrity < 30)
            return false;

        if (state.Resources[ResourceKind.Fuel] < GameBalance.SignalFireFuelCost)
            return false;

        if (state.Resources[ResourceKind.Materials] < GameBalance.SignalFireMaterialsCost)
            return false;

        return true;
    }

    public void ResolveNow(GameState state, ResolutionEntry entry)
    {
        ResolveWithResponse("too_risky", state, entry);
    }

    public IReadOnlyList<EventResponse> GetResponses(GameState state) =>
    [
        new EventResponse("light", "Light the signal fires",
            $"Costs {GameBalance.SignalFireFuelCost} fuel + {GameBalance.SignalFireMaterialsCost} materials. -1 day off relief arrival. +5 unrest."),
        new EventResponse("too_risky", "Too risky"),
    ];

    public void ResolveWithResponse(string responseId, GameState state, ResolutionEntry entry)
    {
        if (responseId == "light")
        {
            state.AddResource(ResourceKind.Fuel, -GameBalance.SignalFireFuelCost, entry);
            state.AddResource(ResourceKind.Materials, -GameBalance.SignalFireMaterialsCost, entry);
            state.ReliefAcceleration++;
            state.AddUnrest(5, entry);
            state.SignalFireLit = true;
            entry.Write("The signal fires blaze atop the keep. The smoke can be seen for miles. If anyone is coming, they will see it. So will the enemy.");
        }
        else
        {
            entry.Write("You decide the risk is too great. The fires remain unlit.");
        }
    }
}
