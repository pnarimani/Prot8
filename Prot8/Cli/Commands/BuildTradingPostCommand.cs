using Prot8.Buildings;
using Prot8.Constants;
using Prot8.Resources;
using Prot8.Simulation;
using Prot8.Zones;

namespace Prot8.Cli.Commands;

public sealed class BuildTradingPostCommand : ICommand
{
    public CommandResult Execute(CommandContext context)
    {
        if (!GameBalance.EnableTradingPost)
            return new CommandResult(false, "Trading post is not enabled.");

        var state = context.State;

        if (state.TradingPostBuilt)
            return new CommandResult(false, "Trading post is already built.");

        if (!state.Resources.Has(ResourceKind.Materials, GameBalance.TradingPostBuildCost))
            return new CommandResult(false,
                $"Not enough materials. Need {GameBalance.TradingPostBuildCost}, have {state.Resources[ResourceKind.Materials]}.");

        var innerDistrict = state.GetZone(ZoneId.InnerDistrict);
        if (innerDistrict.IsLost)
            return new CommandResult(false, "Inner District is lost. Cannot build Trading Post.");

        state.Resources.Consume(ResourceKind.Materials, GameBalance.TradingPostBuildCost);
        state.TradingPostBuilt = true;

        var tradingPost = state.GetBuilding(BuildingId.TradingPost);
        tradingPost.IsActive = true;

        return new CommandResult(true,
            $"Trading Post built in Inner District. Materials -{GameBalance.TradingPostBuildCost}. Assign workers to enable trades.");
    }
}
