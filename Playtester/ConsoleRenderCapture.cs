using Prot8.Cli.Output;
using Prot8.Jobs;
using Prot8.Simulation;

namespace Playtester;

internal static class ConsoleRenderCapture
{
    public static string RenderDayStart(GameState state)
    {
        return Capture(renderer => renderer.RenderDayStart(state));
    }

    public static string RenderPendingPlan(GameState state, JobAllocation allocation, TurnActionChoice action)
    {
        return Capture(renderer => renderer.RenderPendingPlan(state, allocation, action));
    }

    public static string RenderDayReport(GameState state, DayResolutionReport report)
    {
        return Capture(renderer => renderer.RenderDayReport(state, report));
    }

    private static string Capture(Action<ConsoleRenderer> renderAction)
    {
        var originalOut = Console.Out;
        using var writer = new StringWriter();

        try
        {
            Console.SetOut(writer);
            var renderer = new ConsoleRenderer();
            renderAction(renderer);
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        return writer.ToString().TrimEnd();
    }
}
