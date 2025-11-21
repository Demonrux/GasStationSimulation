using GasStation.Core.Models;
using GasStation.Core.Workers;
using GasStation.Core.Utils;
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

            var economyManager = new EconomyManager(
                fuelPurchasePrice: config.FuelPurchasePrice,
                fuelSellPrice: config.FuelSellPrice,
                refuellerSalary: config.RefuellerSalaryPerCar,
                cashierSalary: config.CashierSalaryPerCar,
                initialBalance: config.InitialBalance
            );

            var refuelServices = new List<IService<Car>>();
            for (int i = 0; i < config.RefuellerCount; i++)
            {
                refuelServices.Add(new RefuelService(refuelQueue, refuellerPool, fuelTank, logger));
            }

            var paymentServices = new List<IService<Car>>();
            for (int i = 0; i < config.CashierCount; i++)
            {
                paymentServices.Add(new PaymentService(paymentQueue, cashierPool, logger));
            }

            refuellerPool.SubscribeToWorkEvents(
                onWorkStarted: (workerId, carId) => logger.LogInfo($"Заправщик {workerId} начал заправку машины {carId}. Топливо: {fuelTank.CurrentLevel}л"),
                onWorkCompleted: (workerId, carId, duration) =>
                {
                    logger.LogInfo($"Заправщик {workerId} закончил заправку машины {carId} за {duration.TotalSeconds}с");
                }
            );

            cashierPool.SubscribeToWorkEvents(
                onWorkStarted: (workerId, carId) => logger.LogInfo($"Кассир {workerId} начал обслуживание машины {carId}"),
                onWorkCompleted: (workerId, carId, duration) =>
                {
                    logger.LogInfo($"Кассир {workerId} закончил обслуживание машины {carId} за {duration.TotalSeconds}с");
                }
            );

            return new GasStationEngine(config, logger, fuelTank, refuelQueue, paymentQueue,
                refuellerPool, cashierPool, carGenerator, fuelTruckGenerator,
                refuelServices, paymentServices, economyManager);
        }
    }
}
