namespace GasStation.FileOperations.Classes
{
    public class SimulationConfig
    {
        public int SimulationDurationSeconds { get; set; }
        public int CarGenerationInterval { get; set; }
        public int FuelTruckGenerationInterval { get; set; }
        public int RefuellerCount { get; set; }
        public int CashierCount { get; set; }
        public int FuelTankCapacity { get; set; }
        public int InitialFuelLevel { get; set; }
        public decimal FuelPurchasePrice { get; set; }
        public decimal FuelSellPrice { get; set; }
        public decimal RefuellerSalaryPerCar { get; set; }
        public decimal CashierSalaryPerCar { get; set; }
        public decimal InitialBalance { get; set; }
    }
}