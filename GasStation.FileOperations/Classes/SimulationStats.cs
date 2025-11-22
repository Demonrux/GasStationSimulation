using GasStation.Core.Utils;

namespace GasStation.FileOperations.Classes
{
    public class SimulationStats
    {
        public int TotalCarsGenerated { get; set; }
        public int TotalCarsRefueled { get; set; }     
        public int TotalCarsPaid { get; set; }          
        public int TotalCompletedCars { get; set; }    
        public int TotalFuelTrucks { get; set; }
        public int TotalFuelDelivered { get; set; }
        public int FinalFuelLevel { get; set; }
        public int MaxQueueLengthRefuel { get; set; }
        public int MaxQueueLengthPayment { get; set; }
        public Dictionary<int, int> RefuellerStats { get; set; } = new();
        public Dictionary<int, int> CashierStats { get; set; } = new();
        public EconomyStats EconomyStats { get; set; }
        public TimeSpan SimulationDuration { get; set; }
    }
}
