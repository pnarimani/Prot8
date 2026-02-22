namespace Prot8.Resources;

public readonly record struct ResourceQuantity(ResourceKind Resource, double Quantity)
{
    public override string ToString()
    {
        return $"{Quantity}x {Resource}";
    }
}