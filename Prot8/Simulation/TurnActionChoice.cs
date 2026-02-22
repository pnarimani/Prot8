namespace Prot8.Simulation;

public sealed class TurnActionChoice
{
    public string? LawId { get; set; }
    public string? EmergencyOrderId { get; set; }
    public string? MissionId { get; set; }

    public bool HasAction => !string.IsNullOrWhiteSpace(LawId) || !string.IsNullOrWhiteSpace(EmergencyOrderId) || !string.IsNullOrWhiteSpace(MissionId);
}