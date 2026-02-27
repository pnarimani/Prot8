using Prot8.Resources;

namespace Prot8.Scavenging;

public sealed record ScavengingReward(ResourceKind Resource, int Min, int Max);
