namespace Prot8.Simulation;

public static class FormattingExtenions
{
    public static string ToPercent(this double multiplier)
    {
        return $"{(multiplier - 1) * 100:+0;-0}%";
    }
}