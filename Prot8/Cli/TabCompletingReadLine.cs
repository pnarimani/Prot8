using Prot8.Cli.ViewModels;
using Prot8.Jobs;

namespace Prot8.Cli.Input;

public static class TabCompletingReadLine
{
    static readonly string[] CommandNames =
    [
        "assign", "clear_action", "clear_assignments", "decree",
        "enact", "end_day", "help", "mission", "order", "view"
    ];

    static readonly string[] ViewTabNames = ["laws", "orders", "missions", "decrees"];

    public static (string? line, ActionTab? tabSwitch) ReadLine(DayStartViewModel vm)
    {
        if (Console.IsInputRedirected)
            return (Console.ReadLine(), null);

        var buffer = new List<char>();
        var cursorPos = 0;
        var completionIndex = -1;
        string[]? completionCandidates = null;
        string? completionSnapshot = null;
        var lineStartLeft = Console.CursorLeft;
        var lineStartTop = Console.CursorTop;

        void Redraw()
        {
            Console.SetCursorPosition(lineStartLeft, lineStartTop);
            var text = new string(buffer.ToArray());
            Console.Write(text);
            // Clear any leftover characters
            var clearLen = 40;
            Console.Write(new string(' ', clearLen));
            Console.SetCursorPosition(lineStartLeft + cursorPos, lineStartTop);
        }

        void RedrawWithGhost(string ghost)
        {
            Console.SetCursorPosition(lineStartLeft, lineStartTop);
            var text = new string(buffer.ToArray());
            Console.Write(text);
            // Write ghost text in grey
            Console.Write($"\x1b[90m ({ghost})\x1b[0m");
            // Clear any leftover
            Console.Write("     ");
            Console.SetCursorPosition(lineStartLeft + cursorPos, lineStartTop);
        }

        while (true)
        {
            var key = Console.ReadKey(intercept: true);

            switch (key.Key)
            {
                case ConsoleKey.Enter:
                    Redraw(); // clear ghost text
                    Console.WriteLine();
                    return (new string(buffer.ToArray()), null);

                case ConsoleKey.Escape:
                    buffer.Clear();
                    cursorPos = 0;
                    completionCandidates = null;
                    Redraw();
                    break;

                case ConsoleKey.Backspace:
                    if (cursorPos > 0)
                    {
                        buffer.RemoveAt(cursorPos - 1);
                        cursorPos--;
                        completionCandidates = null;
                        Redraw();
                    }
                    break;

                case ConsoleKey.Delete:
                    if (cursorPos < buffer.Count)
                    {
                        buffer.RemoveAt(cursorPos);
                        completionCandidates = null;
                        Redraw();
                    }
                    break;

                case ConsoleKey.LeftArrow:
                    if (cursorPos > 0)
                    {
                        cursorPos--;
                        Console.SetCursorPosition(lineStartLeft + cursorPos, lineStartTop);
                    }
                    break;

                case ConsoleKey.RightArrow:
                    if (cursorPos < buffer.Count)
                    {
                        cursorPos++;
                        Console.SetCursorPosition(lineStartLeft + cursorPos, lineStartTop);
                    }
                    break;

                case ConsoleKey.Tab:
                    var (_, wordStart) = GetCurrentWord(buffer, cursorPos);
                    var currentText = new string(buffer.ToArray());

                    if (completionCandidates is null || completionSnapshot != currentText)
                    {
                        completionCandidates = GetCompletions(buffer, vm);
                        completionIndex = -1;
                        completionSnapshot = currentText;
                    }

                    if (completionCandidates.Length == 0)
                        break;

                    completionIndex = (completionIndex + 1) % completionCandidates.Length;
                    var completion = completionCandidates[completionIndex];

                    // Replace word from wordStart to cursorPos
                    var charsToRemove = cursorPos - wordStart;
                    buffer.RemoveRange(wordStart, charsToRemove);
                    var completionChars = completion.ToCharArray();
                    buffer.InsertRange(wordStart, completionChars);
                    cursorPos = wordStart + completionChars.Length;

                    // After completing, update snapshot to new text so next Tab cycles
                    // but keep candidates list stable
                    completionSnapshot = new string(buffer.ToArray());

                    if (completionCandidates.Length > 1)
                    {
                        var nextIdx = (completionIndex + 1) % completionCandidates.Length;
                        RedrawWithGhost(completionCandidates[nextIdx]);
                    }
                    else
                    {
                        Redraw();
                    }

                    // Tab auto-switch: if user is typing a command prefix, switch the active tab
                    var tabSwitch = DetectTabSwitch(buffer);
                    if (tabSwitch.HasValue)
                        return (null, tabSwitch);

                    break;

                default:
                    if (key.KeyChar >= 32)
                    {
                        buffer.Insert(cursorPos, key.KeyChar);
                        cursorPos++;
                        completionCandidates = null;
                        Redraw();
                    }
                    break;
            }
        }
    }

    static string[] GetCompletions(List<char> buffer, DayStartViewModel vm)
    {
        var text = new string(buffer.ToArray());
        var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // Completing command name (first word)
        if (parts.Length == 0 || (parts.Length == 1 && !text.EndsWith(' ')))
        {
            var partial = parts.Length == 1 ? parts[0] : "";
            return CommandNames
                .Where(c => c.StartsWith(partial, StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }

        // Completing argument (second word)
        var command = parts[0].ToLowerInvariant();
        var argPartial = parts.Length >= 2 && !text.EndsWith(' ') ? parts[1] : "";

        return command switch
        {
            "enact" => vm.AvailableLaws
                .Where(l => !l.IsActive)
                .Where(l => l.Id.StartsWith(argPartial, StringComparison.OrdinalIgnoreCase))
                .Select(l => l.Id)
                .ToArray(),

            "order" => vm.AvailableOrders
                .Where(o => o.Id.StartsWith(argPartial, StringComparison.OrdinalIgnoreCase))
                .Select(o => o.Id)
                .ToArray(),

            "mission" => vm.AvailableMissions
                .Where(m => m.Id.StartsWith(argPartial, StringComparison.OrdinalIgnoreCase))
                .Select(m => m.Id)
                .ToArray(),

            "decree" => vm.AvailableDecrees
                .Where(d => d.Id.StartsWith(argPartial, StringComparison.OrdinalIgnoreCase))
                .Select(d => d.Id)
                .ToArray(),

            "assign" => Enum.GetNames<JobType>()
                .Where(j => j.StartsWith(argPartial, StringComparison.OrdinalIgnoreCase))
                .ToArray(),

            "view" => ViewTabNames
                .Where(t => t.StartsWith(argPartial, StringComparison.OrdinalIgnoreCase))
                .ToArray(),

            _ => []
        };
    }

    static ActionTab? DetectTabSwitch(List<char> buffer)
    {
        var text = new string(buffer.ToArray()).TrimStart();
        // Only trigger on "command " with trailing space (user pressed Tab after command, before typing arg)
        if (text.Equals("enact ", StringComparison.OrdinalIgnoreCase))
            return ActionTab.Laws;
        if (text.Equals("order ", StringComparison.OrdinalIgnoreCase))
            return ActionTab.Orders;
        if (text.Equals("mission ", StringComparison.OrdinalIgnoreCase))
            return ActionTab.Missions;
        if (text.Equals("decree ", StringComparison.OrdinalIgnoreCase))
            return ActionTab.Decrees;
        return null;
    }

    static (string prefix, int wordStart) GetCurrentWord(List<char> buffer, int cursorPos)
    {
        var start = cursorPos;
        while (start > 0 && buffer[start - 1] != ' ')
            start--;
        var word = new string(buffer.Skip(start).Take(cursorPos - start).ToArray());
        return (word, start);
    }
}
