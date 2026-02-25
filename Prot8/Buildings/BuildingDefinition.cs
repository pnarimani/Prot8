using Prot8.Resources;
using Prot8.Zones;

namespace Prot8.Buildings;

public sealed record BuildingDefinition(
    BuildingId Id,
    string Name,
    ZoneId Zone,
    int MaxWorkers,
    IReadOnlyList<ResourceQuantity> Inputs,
    IReadOnlyList<ResourceQuantity> Outputs);
