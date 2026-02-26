using Prot8.Resources;

namespace Prot8.Trading;

public sealed record TradeOffer(ResourceKind SourceResource, ResourceKind TargetResource, int Amount);
