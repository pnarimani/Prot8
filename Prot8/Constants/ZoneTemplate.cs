using Prot8.Zones;

namespace Prot8.Constants;

public sealed record ZoneTemplate(ZoneId ZoneId, string Name, int StartingIntegrity, int StartingCapacity, int StartingPopulation, double PerimeterFactor);