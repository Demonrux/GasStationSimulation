using GasStation.Core.Models;
using GasStation.Core.Workers;
using GasStation.FileOperations.Interfaces;
using GasStation.FileOperations.Classes;
using GasStation.Services.Classes;
using GasStation.Services.Generators;
using GasStation.Services.Interfaces;
using GasStation.Services;

namespace GasStation.Engine.Classes
{
    public static class GasStationFactory
    {
        public static GasStationEngine Create(SimulationConfig config, ILogger logger)
        {
            var fuelTank = new FuelTank(config.FuelTankCapacity, config.InitialFuelLevel);
            var refuelQueue = new CarQueue();
            var paymentQueue = new CarQueue();

            var refuellers = Enumerable.Range(1, config.RefuellerCount).Select(id => new Refueller(id, fuelTank)).ToList();
            var cashiers = Enumerable.Range(1, config.CashierCount).Select(id => new Cashier(id)).ToList();

            var refuellerPool = new WorkerPool<Refueller>(refuellers);
            var cashierPool = new WorkerPool<Cashier>(cashiers);

            var carGenerator = new CarGenerator(TimeSpan.FromSeconds(config.CarGenerationInterval), logger);
            var fuelTruckGenerator = new FuelTruckGenerator(TimeSpan.FromSeconds(config.FuelTruckGenerationInterval), logger);

            IService<Car> refuelService = new RefuelService(refuelQueue, refuellerPool, fuelTank, logger);
            IService<Car> paymentService = new PaymentService(paymentQueue, cashierPool, logger);

            return new GasStationEngine(config, logger, fuelTank, refuelQueue, paymentQueue,
                refuellerPool, cashierPool, carGenerator, fuelTruckGenerator,
                refuelService, paymentService);
        }

    }
}
