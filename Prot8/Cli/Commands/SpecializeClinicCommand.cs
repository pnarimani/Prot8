using System.Text.Json.Serialization;
using Prot8.Buildings;
using Prot8.Constants;
using Prot8.Resources;
using Prot8.Simulation;

namespace Prot8.Cli.Commands;

public sealed class SpecializeClinicCommand : ICommand
{
    [JsonPropertyName("specialization")]
    public required string SpecializationStr { get; init; }

    public CommandResult Execute(CommandContext context)
    {
        if (!GameBalance.EnableClinicSpecialization)
        {
            return new CommandResult(false, "Clinic specialization is not enabled.");
        }

        if (context.State.ClinicSpecialization != ClinicSpecialization.None)
        {
            return new CommandResult(false,
                $"Clinic is already specialized as {context.State.ClinicSpecialization}. This is a permanent choice.");
        }

        var clinic = context.State.GetBuilding(BuildingId.Clinic);
        if (clinic.IsDestroyed)
        {
            return new CommandResult(false, "The Clinic is destroyed and cannot be specialized.");
        }

        if (!TryParseSpecialization(SpecializationStr, out var spec))
        {
            return new CommandResult(false,
                $"Unknown specialization '{SpecializationStr}'. Valid options: hospital, quarantine_ward.");
        }

        if (!context.State.Resources.Has(ResourceKind.Materials, GameBalance.ClinicSpecializationMaterialsCost))
        {
            return new CommandResult(false,
                $"Not enough materials. Need {GameBalance.ClinicSpecializationMaterialsCost}, have {context.State.Resources[ResourceKind.Materials]}.");
        }

        context.State.Resources.Consume(ResourceKind.Materials, GameBalance.ClinicSpecializationMaterialsCost);
        context.State.ClinicSpecialization = spec;

        var description = spec == ClinicSpecialization.Hospital
            ? $"Recovery capacity +{GameBalance.HospitalRecoveryBonus * 100:F0}%"
            : $"Sickness delta -{GameBalance.QuarantineWardSicknessReduction}/day";

        return new CommandResult(true,
            $"Clinic specialized as {spec}. {description}. Materials -{GameBalance.ClinicSpecializationMaterialsCost}.");
    }

    static bool TryParseSpecialization(string input, out ClinicSpecialization result)
    {
        result = input.ToLowerInvariant() switch
        {
            "hospital" => ClinicSpecialization.Hospital,
            "quarantine_ward" => ClinicSpecialization.QuarantineWard,
            _ => ClinicSpecialization.None,
        };
        return result != ClinicSpecialization.None;
    }
}
