using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Playtester;
using Prot8.Cli.Commands;
using Prot8.Cli.Output;
using Prot8.Cli.ViewModels;
using Prot8.Simulation;
using Prot8.Telemetry;

var config = ParseArgs(args);

Console.WriteLine("Prot8 AI Playtester");
Console.WriteLine($"  Endpoint: {config.Endpoint}");
Console.WriteLine($"  Model:    {(string.IsNullOrEmpty(config.Model) ? "(server default)" : config.Model)}");
Console.WriteLine($"  Seed:     {config.Seed?.ToString() ?? "random"}");
Console.WriteLine();

await OperatorAnalystRunner.RunAsync(config);


static PlaytesterConfig ParseArgs(string[] args)
{
    var config = new PlaytesterConfig();
    foreach (var arg in args)
    {
        if (arg.StartsWith("--seed=", StringComparison.OrdinalIgnoreCase))
        {
            if (int.TryParse(arg["--seed=".Length..], out var s))
            {
                config.Seed = s;
            }
        }
        else if (arg.StartsWith("--model=", StringComparison.OrdinalIgnoreCase))
        {
            config.Model = arg["--model=".Length..];
        }
        else if (arg.StartsWith("--endpoint=", StringComparison.OrdinalIgnoreCase))
        {
            config.Endpoint = arg["--endpoint=".Length..];
        }
    }

    return config;
}

public class PlaytesterConfig
{
    public int? Seed { get; set; }
    public string? Model { get; set; }
    public string Endpoint { get; set; } = "http://localhost:1234";
}