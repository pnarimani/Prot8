using Prot8.Constants;
using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Diplomacy;

public sealed class BetrayAlliesAction : IDiplomaticAction
{
    public string Id => "betray_allies";
    public string Name => "Betray Allies";
    public bool CanDeactivate => false;

    public string GetTooltip(GameState state) =>
        $"One-time: +{GameBalance.BetrayalFood} food, +{GameBalance.BetrayalWater} water, +{GameBalance.BetrayalMaterials} materials. " +
        $"+{GameBalance.BetrayalUnrest} unrest, {GameBalance.BetrayalMorale} morale. {GameBalance.BetrayalRetaliationChance}% daily retaliation. " +
        $"Cannot deactivate. Requires Tyranny >= 4.";

    public bool CanActivate(GameState state) => state.Flags.Tyranny >= 4;

    public void OnActivate(GameState state, ResolutionEntry entry)
    {
        state.AddResource(ResourceKind.Food, GameBalance.BetrayalFood, entry);
        state.AddResource(ResourceKind.Water, GameBalance.BetrayalWater, entry);
        state.AddResource(ResourceKind.Materials, GameBalance.BetrayalMaterials, entry);
        state.AddUnrest(GameBalance.BetrayalUnrest, entry);
        state.AddMorale(GameBalance.BetrayalMorale, entry);
        entry.Write("You sell out your allies to the besiegers. The windfall is enormous, but trust is shattered forever.");
    }

    public void ApplyDaily(GameState state, ResolutionEntry entry)
    {
        if (state.RollPercent() <= GameBalance.BetrayalRetaliationChance)
        {
            var retaliationType = state.Random.Next(0, 3);
            switch (retaliationType)
            {
                case 0:
                    state.AddUnrest(8, entry);
                    entry.Write("Word of the betrayal spreads. Former allies stir up hatred among the people.");
                    break;
                case 1:
                    state.ApplyDeath(2, entry);
                    entry.Write("Retaliatory strike from betrayed allies kills 2.");
                    break;
                case 2:
                    state.AddResource(ResourceKind.Food, -10, entry);
                    entry.Write("Former allies poison a food shipment. -10 food.");
                    break;
            }
        }
    }
}
