using GasStation.Core.Enums;

namespace GasStation.Core.Models
{
    public class Car
    {
        public int Id { get; }
        public CarState State { get; set; }
        public int RequiredFuel { get; }

        public Car(int id)
        {
            Id = id;
            State = CarState.WaitingForRefuel;
            RequiredFuel = Random.Shared.Next(Constants.MinCarFuelAmount, Constants.MaxCarFuelAmount);
        }
    }
}