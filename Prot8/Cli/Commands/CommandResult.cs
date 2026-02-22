namespace Prot8.Cli.Input.Commands;

public sealed record CommandResult(bool Success, string Message, bool EndDayRequested = false);
