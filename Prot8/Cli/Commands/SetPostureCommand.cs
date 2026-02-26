using System.Text.Json.Serialization;
using Prot8.Constants;
using Prot8.Simulation;

namespace Prot8.Cli.Commands;

public sealed class SetPostureCommand : ICommand
{
    [JsonPropertyName("posture")]
    public required string PostureStr { get; init; }

    public CommandResult Execute(CommandContext context)
    {
        if (!GameBalance.EnableDefensivePosture)
            return new CommandResult(false, "Defensive posture system is not enabled.");

        if (!TryParsePosture(PostureStr, out var posture))
            return new CommandResult(false,
                $"Unknown posture '{PostureStr}'. Valid options: none, hunker_down, active_defense, aggressive_patrols, open_gates, scorched_perimeter.");

        var state = context.State;

        if (posture == DefensivePosture.ScorchedPerimeter)
        {
            var perimeter = state.ActivePerimeterZone;
            if (state.ScorchedPerimeterUsed.ContainsKey(perimeter.Id))
                return new CommandResult(false, $"Scorched Perimeter already used on {perimeter.Name}. Can only be used once per zone.");
        }

        if (posture is DefensivePosture.ActiveDefense or DefensivePosture.AggressivePatrols)
        {
            var availableGuards = state.Population.Guards - state.ReservedGuardsForMissions;
            if (availableGuards < GameBalance.DefensivePostureGuardMinimum)
                return new CommandResult(false,
                    $"Not enough available guards. Need at least {GameBalance.DefensivePostureGuardMinimum}, have {availableGuards}.");
        }

        state.CurrentPosture = posture;
        var description = posture switch
        {
            DefensivePosture.None => "Normal operations resumed.",
            DefensivePosture.HunkerDown => $"Hunkered down. Siege damage -{GameBalance.HunkerDownSiegeReduction * 100:F0}%, no missions allowed.",
            DefensivePosture.ActiveDefense => $"Active defense. Siege damage -{GameBalance.ActiveDefenseSiegeReduction * 100:F0}%, guards committed to walls.",
            DefensivePosture.AggressivePatrols => $"Aggressive patrols. -{GameBalance.AggressivePatrolsUnrest}/day unrest, {GameBalance.AggressivePatrolsInterceptChance}% resource interception chance.",
            DefensivePosture.OpenGates => $"Gates opened. +{GameBalance.OpenGatesMorale} morale, {GameBalance.OpenGatesRefugeeChance}% refugee chance, {GameBalance.OpenGatesInfiltratorChance}% infiltrator risk.",
            DefensivePosture.ScorchedPerimeter => "Scorched perimeter activated.",
            _ => "Posture set."
        };

        return new CommandResult(true, description);
    }

    static bool TryParsePosture(string input, out DefensivePosture result)
    {
        result = input.ToLowerInvariant() switch
        {
            "none" => DefensivePosture.None,
            "hunker_down" => DefensivePosture.HunkerDown,
            "active_defense" => DefensivePosture.ActiveDefense,
            "aggressive_patrols" => DefensivePosture.AggressivePatrols,
            "open_gates" => DefensivePosture.OpenGates,
            "scorched_perimeter" => DefensivePosture.ScorchedPerimeter,
            _ => (DefensivePosture)(-1),
        };
        return (int)result >= 0;
    }
}
