using System.Text.Json;
using System.Text.Json.Nodes;
using Prot8.Cli.Commands;

namespace Playtester;

public class Parsing
{
    public static List<ICommand> ParseCommanderCommands(string json, JsonSerializerOptions jsonSerializationOptions)
    {
        try
        {
            var node = JsonNode.Parse(json);
            var array = node?["commands"]?.AsArray();
            if (array is null)
            {
                return [];
            }

            var result = new List<ICommand>();
            foreach (var item in array)
            {
                if (item is null)
                {
                    continue;
                }

                try
                {
                    var command = item.Deserialize<ICommand>(jsonSerializationOptions);
                    if (command is not null)
                    {
                        result.Add(command);
                    }
                }
                catch (JsonException ex)
                {
                    Console.WriteLine(
                        $"[AI] Warning: failed to deserialize command '{item}': {ex.Message}, skipping.");
                }
            }

            return result;
        }
        catch
        {
            Console.WriteLine("[AI] Warning: failed to parse Commander JSON, treating as empty.");
            return [];
        }
    }
}