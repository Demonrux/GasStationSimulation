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
    }
}