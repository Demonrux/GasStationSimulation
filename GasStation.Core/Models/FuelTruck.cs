namespace GasStation.Core.Models
{
    public class FuelTruck
    {
        public int Id { get; }
        public int FuelAmount { get; }

        public FuelTruck(int id, int fuelAmount)
        {
            Id = id;
            FuelAmount = fuelAmount;
        }
    }
}