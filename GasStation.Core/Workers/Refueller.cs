using GasStation.Core.Models;

namespace GasStation.Core.Workers
{
    public class Refueller : Worker
    {
        private readonly FuelTank _fuelTank;

        public Refueller(int id, FuelTank fuelTank) : base(id, "Refueller")
        {
            _fuelTank = fuelTank;
        }
    }
}