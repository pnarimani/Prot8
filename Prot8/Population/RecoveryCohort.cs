namespace Prot8.Population;

public sealed class RecoveryCohort
{
    public RecoveryCohort(int count, int daysRemaining)
    {
        Count = count;
        DaysRemaining = daysRemaining;
    }

    public int Count { get; set; }

    public int DaysRemaining { get; set; }
}