using GasStation.Core.Workers;
using GasStation.Core.Models;
using GasStation.Core.Utils;
using GasStation.FileOperations.Interfaces;
using GasStation.FileOperations.Classes;
using GasStation.Engine.Interfaces;
using GasStation.Services.Interfaces;

namespace GasStation.Engine.Classes
{
    public class GasStationEngine : IEngine
    {
        private readonly object _lock = new object();
        private readonly SimulationConfig _config;
        private readonly SimulationStats _stats = new();
        private DateTime _simulationStartTime;
        private readonly FuelTank _fuelTank;
        private readonly CarQueue _refuelQueue;
        private readonly CarQueue _paymentQueue;
        private readonly WorkerPool<Refueller> _refuellerPool;
        private readonly WorkerPool<Cashier> _cashierPool;
        private readonly IGenerator<Car> _carGenerator;
        private readonly IGenerator<FuelTruck> _fuelTruckGenerator;
        private readonly List<IService<Car>> _refuelServices;
        private readonly List<IService<Car>> _paymentServices; 
        private readonly ILogger _logger;
        private readonly EconomyManager _economyManager;

        private CancellationTokenSource _cancellationTokenSourse;
        private bool _isRunning;

        public GasStationEngine(
            SimulationConfig config,
            ILogger logger,
            FuelTank fuelTank,
            CarQueue refuelQueue,
            CarQueue paymentQueue,
            WorkerPool<Refueller> refuellerPool,
            WorkerPool<Cashier> cashierPool,
            IGenerator<Car>  carGenerator,
            IGenerator<FuelTruck> fuelTruckGenerator,
            List<IService<Car>> refuelServices,
            List<IService<Car>> paymentServices,
            EconomyManager economyManager)
        {
            _config = config;
            _fuelTank = fuelTank;
            _refuelQueue = refuelQueue;
            _paymentQueue = paymentQueue;
            _refuellerPool = refuellerPool;
            _cashierPool = cashierPool;
            _carGenerator = carGenerator;
            _fuelTruckGenerator = fuelTruckGenerator;
            _refuelServices = refuelServices;
            _paymentServices = paymentServices;
            _logger = logger;
            _economyManager = economyManager;

            SetupEvents();
        }

        private void SetupEvents()
        {
            _carGenerator.Generated += OnCarGenerated;
            _fuelTruckGenerator.Generated += OnFuelTruckGenerated;
        
            foreach (var refuelService in _refuelServices)
            {
                refuelService.ItemProcessed += OnCarRefueled;
            }
        
            foreach (var paymentService in _paymentServices)
            {
                paymentService.ItemProcessed += OnCarPaid;
            }
        
            _fuelTank.FuelLevelChanged += OnFuelLevelChanged;
        }

         private void OnCarGenerated(Car car)
         {
             _refuelQueue.Enqueue(car);
             _logger.LogInfo($"Машина {car.Id} прибыла на заправку (нужно {car.RequiredFuel}л)");
         }
        
         private void OnFuelTruckGenerated(FuelTruck truck)
         {
             _fuelTank.Refuel(truck.FuelAmount);
             _economyManager.RecordFuelPurchase(truck.FuelAmount);
             _logger.LogInfo($"Бензовоз {truck.Id} доставил {truck.FuelAmount}л " +
                            $"за {truck.FuelAmount * _economyManager.FuelPurchasePrice} RUB " +
                            $"Теперь в резервуаре: {_fuelTank.CurrentLevel}л");
         }
        
         private void OnCarRefueled(Car car)
         {
            _economyManager.RecordFuelSale(car.RequiredFuel, car.Id);
            _logger.LogInfo($"Машина {car.Id} заправлена на {car.RequiredFuel}л " +
                           $"за {car.RequiredFuel * _economyManager.FuelSellPrice} RUB, переходит на оплату");
            _paymentQueue.Enqueue(car);
         }

         private void OnCarPaid(Car car)
         {
             _logger.LogInfo($"Машина {car.Id} обслужена и уезжает");
         }
        
         private void OnFuelLevelChanged(int level)
         {
             if (level < Constants.FuelThreshold)
                 _logger.LogWarning($"Низкий уровень топлива: {level}л");
         }

        public async Task RunSimulation()
        {
            lock (_lock)
            {
                if (_isRunning)
                    throw new InvalidOperationException("Моделирование уже запущено");
                _isRunning = true;
                _cancellationTokenSourse = new CancellationTokenSource();
                _simulationStartTime = DateTime.Now;
            }

            try
            {
                var tasks = new List<Task>
                {
                    Task.Run(() => _carGenerator.StartGeneration(_cancellationTokenSourse.Token)), 
                    Task.Run(() => _fuelTruckGenerator.StartGeneration(_cancellationTokenSourse.Token)),

                    MonitorSimulation()
                };

                foreach (var refuelService in _refuelServices)
                {
                    tasks.Add(Task.Run(() => refuelService.Process(_cancellationTokenSourse.Token)));
                }

                foreach (var paymentService in _paymentServices)
                {
                    tasks.Add(Task.Run(() => paymentService.Process(_cancellationTokenSourse.Token)));
                }

                await Task.WhenAll(tasks);

                CollectAndLogStatistics();
            }
            catch (OperationCanceledException) 
            {
                _logger.LogWarning("Моделирование остановлено");
                CollectAndLogStatistics();
            }

            catch (Exception exception)
            {
                _logger.LogError($"{exception.Message}");
            }
            finally
            {
                _logger.LogInfo("Моделирование завершено");
                _isRunning = false;
            }
        }

        private void CollectAndLogStatistics()
        {
            try
            {
                _stats.SimulationDuration = DateTime.Now - _simulationStartTime;

                _stats.TotalCarsGenerated = _carGenerator.GeneratedCount;
                _stats.TotalFuelTrucks = _fuelTruckGenerator.GeneratedCount;
                _stats.FinalFuelLevel = _fuelTank.CurrentLevel;

                _stats.MaxQueueLengthRefuel = _refuelQueue.MaxQueueLength;
                _stats.MaxQueueLengthPayment = _paymentQueue.MaxQueueLength;

                _stats.RefuellerStats = _refuellerPool.GetWorkers().ToDictionary(workers => workers.Id,workers => workers.ProcessedCount);
                _stats.CashierStats = _cashierPool.GetWorkers().ToDictionary(workers => workers.Id,workers => workers.ProcessedCount);

                _stats.TotalFuelDelivered = _fuelTank.TotalFuelDelivered;

                _stats.TotalCarsRefueled = _stats.RefuellerStats.Values.Sum();
                _stats.TotalCarsPaid = _stats.CashierStats.Values.Sum();

                _stats.EconomyStats = _economyManager.GetStats();

                _economyManager.PayRefuellerSalary(_stats.TotalCarsRefueled);
                _economyManager.PayCashierSalary(_stats.TotalCarsPaid);

                _logger.LogStatistics(_stats);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при сборе статистики: {ex.Message}");
            }
        }

        public async Task MonitorSimulation()
        {
            await Task.Delay(TimeSpan.FromSeconds(_config.SimulationDurationSeconds), _cancellationTokenSourse.Token);
            Stop();
        }

        public void Stop()
        {
            lock (_lock)
            {
                if (!_isRunning) return;
                _isRunning = false;
            }

            _logger.LogWarning("Остановка моделирования...");
            _cancellationTokenSourse.Cancel();
            _isRunning = false;
        }
    }
}
