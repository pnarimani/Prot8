using Prot8.Resources;
using Prot8.Zones;

namespace Prot8.Buildings;

public sealed class BuildingState(BuildingDefinition definition)
{
    public BuildingId Id => definition.Id;
    public string Name => definition.Name;
    public ZoneId Zone => definition.Zone;
    public int MaxWorkers => definition.MaxWorkers;
    public IReadOnlyList<ResourceQuantity> Inputs => definition.Inputs;
    public IReadOnlyList<ResourceQuantity> Outputs => definition.Outputs;

    public int AssignedWorkers { get; set; }
    public bool IsDestroyed { get; set; }
    public bool IsActive { get; set; } = true;
}
