namespace Prot8.Simulation;

public sealed class DailyProductionResult
{
    public int FoodProduced { get; set; }

    public int WaterProduced { get; set; }

    public int MaterialsProduced { get; set; }

    public int FuelProduced { get; set; }

    public int RepairPoints { get; set; }

    public int ClinicCarePoints { get; set; }

    public int ClinicMedicineSpent { get; set; }

    public int ClinicSlotsUsed { get; set; }
}