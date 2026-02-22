using Prot8.Jobs;
using Prot8.Resources;

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

    public void AddResourceProduction(ResourceKind kind, int amount)
    {
        switch (kind)
        {
            case ResourceKind.Food: FoodProduced += amount; break;
            case ResourceKind.Water: WaterProduced += amount; break;
            case ResourceKind.Materials: MaterialsProduced += amount; break;
            case ResourceKind.Fuel: FuelProduced += amount; break;
        }
    }
}