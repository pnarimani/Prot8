using Prot8.Constants;
using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Diplomacy;

public sealed class SecretCorrespondenceAction : IDiplomaticAction
{
    public string Id => "secret_correspondence";
    public string Name => "Secret Correspondence";
    public bool CanDeactivate => true;

    public string GetTooltip(GameState state) =>
        $"Daily: -{GameBalance.CorrespondenceMaterialsCost} materials. " +
        $"+{GameBalance.CorrespondenceDailyMorale} morale/day, {GameBalance.CorrespondenceIntelChance}% daily intel discovery chance. " +
        $"Requires Faith >= 4.";

    public bool CanActivate(GameState state) => state.Flags.Faith >= 4;

    public void OnActivate(GameState state, ResolutionEntry entry)
    {
        entry.Write("Coded messages are exchanged through the sewers. The faithful maintain contact with allies beyond the walls.");
    }

    public void ApplyDaily(GameState state, ResolutionEntry entry)
    {
        state.AddResource(ResourceKind.Materials, -GameBalance.CorrespondenceMaterialsCost, entry);
        state.AddMorale(GameBalance.CorrespondenceDailyMorale, entry);

        if (state.RollPercent() <= GameBalance.CorrespondenceIntelChance)
        {
            var resourceKinds = new[] { ResourceKind.Food, ResourceKind.Water, ResourceKind.Fuel, ResourceKind.Materials, ResourceKind.Medicine };
            var kind = resourceKinds[state.Random.Next(0, resourceKinds.Length)];
            state.AddResource(kind, GameBalance.CorrespondenceIntelResourceAmount, entry);
            entry.Write($"Intelligence from correspondents reveals hidden supplies: +{GameBalance.CorrespondenceIntelResourceAmount} {kind}.");
        }

        if (GameBalance.EnableReliefArmy
            && state.CorrespondenceAccelerationApplied < GameBalance.CorrespondenceMaxAccel
            && state.RollPercent() <= GameBalance.CorrespondenceAccelChance)
        {
            state.ReliefAcceleration++;
            state.CorrespondenceAccelerationApplied++;
            entry.Write("Coded messages reveal a shortcut for the relief column. Estimated arrival accelerated.");
        }
    }
}
