using System.Text.Json.Serialization;
using Prot8.Buildings;
using Prot8.Constants;
using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Cli.Commands;

public sealed class SpecializeBuildingCommand : ICommand
{
    [JsonPropertyName("building")]
    public required string BuildingStr { get; init; }

    [JsonPropertyName("specialization")]
    public required string SpecializationStr { get; init; }

    public CommandResult Execute(CommandContext context)
    {
        if (!GameBalance.EnableBuildingSpecializations)
            return new CommandResult(false, "Building specializations are not enabled.");

        if (!Enum.TryParse<BuildingId>(BuildingStr, true, out var buildingId))
            return new CommandResult(false, $"Unknown building '{BuildingStr}'.");

        if (!TryParseSpec(SpecializationStr, out var spec))
            return new CommandResult(false, $"Unknown specialization '{SpecializationStr}'.");

        if (!BuildingSpecializationCatalog.IsValidFor(buildingId, spec))
            return new CommandResult(false, $"Specialization {spec} is not valid for {buildingId}.");

        var state = context.State;

        if (state.GetBuildingSpec(buildingId) != BuildingSpecialization.None)
            return new CommandResult(false, $"{buildingId} is already specialized. This is a permanent choice.");

        var building = state.GetBuilding(buildingId);
        if (building.IsDestroyed)
            return new CommandResult(false, $"{building.Name} is destroyed and cannot be specialized.");

        if (!state.Resources.Has(ResourceKind.Materials, GameBalance.BuildingSpecializationMaterialsCost))
            return new CommandResult(false,
                $"Not enough materials. Need {GameBalance.BuildingSpecializationMaterialsCost}, have {state.Resources[ResourceKind.Materials]}.");

        state.Resources.Consume(ResourceKind.Materials, GameBalance.BuildingSpecializationMaterialsCost);
        state.BuildingSpecializations[buildingId] = spec;

        // Migrate clinic specialization for backward compatibility
        if (buildingId == BuildingId.Clinic)
        {
            state.ClinicSpecialization = spec == BuildingSpecialization.Hospital
                ? ClinicSpecialization.Hospital
                : spec == BuildingSpecialization.QuarantineWard
                    ? ClinicSpecialization.QuarantineWard
                    : ClinicSpecialization.None;
        }

        // One-time effects on specialization
        ApplyOnSpecialize(state, buildingId, spec);

        var options = BuildingSpecializationCatalog.GetOptions(buildingId);
        var name = "";
        foreach (var opt in options)
        {
            if (opt.Spec == spec)
            {
                name = opt.Name;
                break;
            }
        }

        return new CommandResult(true,
            $"{building.Name} specialized as {name}. Materials -{GameBalance.BuildingSpecializationMaterialsCost}.");
    }

    static void ApplyOnSpecialize(GameState state, BuildingId buildingId, BuildingSpecialization spec)
    {
        switch (spec)
        {
            case BuildingSpecialization.FortifiedKitchen:
                state.AddMorale(5, new ResolutionEntry { Title = "Specialization" });
                break;
            case BuildingSpecialization.ArmorWorks:
                state.Population.Guards += 2;
                state.Flags.Fortification.Add(1);
                break;
            case BuildingSpecialization.EmergencyReserve:
                state.Resources.Add(ResourceKind.Water, 20);
                break;
            case BuildingSpecialization.WeaponCache:
                state.Flags.Tyranny.Add(1);
                break;
            case BuildingSpecialization.PropagandaPost:
                state.Flags.Faith.Add(1);
                break;
        }
    }

    static bool TryParseSpec(string input, out BuildingSpecialization result)
    {
        // Try direct enum parse
        if (Enum.TryParse(input, true, out result) && result != BuildingSpecialization.None)
            return true;

        // Try snake_case conversion
        var pascalCase = string.Concat(input.Split('_').Select(s =>
            s.Length > 0 ? char.ToUpper(s[0]) + s[1..] : s));
        if (Enum.TryParse(pascalCase, true, out result) && result != BuildingSpecialization.None)
            return true;

        result = BuildingSpecialization.None;
        return false;
    }
}
