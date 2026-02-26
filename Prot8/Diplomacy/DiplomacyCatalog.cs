namespace Prot8.Diplomacy;

public static class DiplomacyCatalog
{
    private static readonly IReadOnlyList<IDiplomaticAction> _allActions =
    [
        new BribeEnemyOfficerAction(),
        new HostageExchangeAction(),
        new OfferTributeAction(),
        new SecretCorrespondenceAction(),
        new BetrayAlliesAction(),
    ];

    public static IReadOnlyList<IDiplomaticAction> GetAll() => _allActions;

    public static IDiplomaticAction? Find(string id)
    {
        foreach (var action in _allActions)
        {
            if (action.Id == id)
                return action;
        }
        return null;
    }
}
