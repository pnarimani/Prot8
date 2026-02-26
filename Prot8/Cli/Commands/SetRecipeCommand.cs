using System.Text.Json.Serialization;
using Prot8.Constants;
using Prot8.Simulation;

namespace Prot8.Cli.Commands;

public sealed class SetRecipeCommand : ICommand
{
    [JsonPropertyName("recipe")]
    public required string RecipeStr { get; init; }

    public CommandResult Execute(CommandContext context)
    {
        if (!GameBalance.EnableKitchenRecipes)
        {
            return new CommandResult(false, "Kitchen recipes are not enabled.");
        }

        if (!TryParseRecipe(RecipeStr, out var recipe))
        {
            return new CommandResult(false,
                $"Unknown recipe '{RecipeStr}'. Valid options: normal, gruel, feast.");
        }

        context.State.ActiveKitchenRecipe = recipe;

        var description = recipe switch
        {
            KitchenRecipe.Gruel => $"Gruel: {GameBalance.GruelFoodPerWorker} food/worker (no fuel), +{GameBalance.GruelSicknessPerDay} sickness/day",
            KitchenRecipe.Feast => $"Feast: {GameBalance.FeastFoodPerWorker} food/worker, {GameBalance.FeastFuelPerWorker} fuel/worker, +{GameBalance.FeastMoralePerDay} morale/day",
            _ => "Normal kitchen operation",
        };

        return new CommandResult(true, $"Kitchen recipe set to {recipe}. {description}.");
    }

    static bool TryParseRecipe(string input, out KitchenRecipe result)
    {
        result = input.ToLowerInvariant() switch
        {
            "normal" => KitchenRecipe.Normal,
            "gruel" => KitchenRecipe.Gruel,
            "feast" => KitchenRecipe.Feast,
            _ => (KitchenRecipe)(-1),
        };
        return (int)result >= 0;
    }
}
