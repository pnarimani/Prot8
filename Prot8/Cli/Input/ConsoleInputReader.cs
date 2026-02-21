using System;
using Prot8.Cli.Output;
using Prot8.Constants;
using Prot8.Jobs;
using Prot8.Laws;
using Prot8.Missions;
using Prot8.Orders;
using Prot8.Simulation;
using Prot8.Zones;

namespace Prot8.Cli.Input;

public sealed class ConsoleInputReader
{
    public JobAllocation ReadJobAllocation(GameState state, ConsoleRenderer renderer)
    {
        renderer.RenderJobMenu(state);

        var allocation = new JobAllocation();
        var remaining = state.AvailableHealthyWorkersForAllocation;

        foreach (var job in Enum.GetValues<JobType>())
        {
            var prompt = $"Assign workers to {job} (remaining {remaining}, step {JobAllocation.Step}): ";
            var assigned = ReadSteppedInt(prompt, remaining, JobAllocation.Step);
            allocation.SetWorkers(job, assigned);
            remaining -= assigned;
        }

        allocation.SetIdleWorkers(remaining);
        Console.WriteLine($"Idle workers today: {remaining}");
        Console.WriteLine();
        return allocation;
    }

    public TurnActionChoice ReadTurnAction(GameState state, ConsoleRenderer renderer)
    {
        renderer.RenderActionMenu();
        var choice = ReadIntInRange("Action choice: ", 0, 3);

        if (choice == 0)
        {
            return new TurnActionChoice();
        }

        if (choice == 1)
        {
            return ReadLawAction(state, renderer);
        }

        if (choice == 2)
        {
            return ReadOrderAction(state, renderer);
        }

        return ReadMissionAction(state, renderer);
    }

    private TurnActionChoice ReadLawAction(GameState state, ConsoleRenderer renderer)
    {
        renderer.RenderLawOptions(state);

        var laws = LawCatalog.GetAll();
        var pick = ReadIntInRange("Select law: ", 0, laws.Count);
        if (pick == 0)
        {
            return new TurnActionChoice();
        }

        var selected = laws[pick - 1];
        return new TurnActionChoice { LawId = selected.Id };
    }

    private TurnActionChoice ReadOrderAction(GameState state, ConsoleRenderer renderer)
    {
        renderer.RenderOrderOptions(state);

        var orders = EmergencyOrderCatalog.GetAll();
        var pick = ReadIntInRange("Select emergency order: ", 0, orders.Count);
        if (pick == 0)
        {
            return new TurnActionChoice();
        }

        var selected = orders[pick - 1];
        var action = new TurnActionChoice { EmergencyOrderId = selected.Id };

        if (!selected.RequiresZoneSelection)
        {
            return action;
        }

        renderer.RenderZoneSelectionPrompt("Select zone:", state.Zones);
        var zoneRaw = ReadIntInRange("Zone: ", 0, 5);
        if (zoneRaw == 0)
        {
            return new TurnActionChoice();
        }

        action.SelectedZoneForOrder = (ZoneId)zoneRaw;
        return action;
    }

    private TurnActionChoice ReadMissionAction(GameState state, ConsoleRenderer renderer)
    {
        renderer.RenderMissionOptions(state);

        var missions = MissionCatalog.GetAll();
        var pick = ReadIntInRange("Select mission: ", 0, missions.Count);
        if (pick == 0)
        {
            return new TurnActionChoice();
        }

        var selected = missions[pick - 1];
        return new TurnActionChoice { MissionId = selected.Id };
    }

    private static int ReadSteppedInt(string prompt, int max, int step)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            if (input is null)
            {
                return 0;
            }

            if (!int.TryParse(input, out var parsed))
            {
                Console.WriteLine("Please enter a number.");
                continue;
            }

            if (parsed < 0 || parsed > max)
            {
                Console.WriteLine($"Value must be between 0 and {max}.");
                continue;
            }

            if (parsed % step != 0)
            {
                Console.WriteLine($"Value must be in increments of {step}.");
                continue;
            }

            return parsed;
        }
    }

    private static int ReadIntInRange(string prompt, int min, int max)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            if (input is null)
            {
                return min;
            }

            if (!int.TryParse(input, out var parsed))
            {
                Console.WriteLine("Please enter a number.");
                continue;
            }

            if (parsed < min || parsed > max)
            {
                Console.WriteLine($"Value must be between {min} and {max}.");
                continue;
            }

            return parsed;
        }
    }
}
