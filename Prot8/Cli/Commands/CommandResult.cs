namespace Prot8.Cli.Commands;

public sealed record CommandResult(bool Success, string Message, bool EndDayRequested = false);
