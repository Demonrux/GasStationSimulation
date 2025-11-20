using GasStation.Core.Workers;
using GasStation.Core.Models;
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
        private readonly IService<Car> _refuelService;
        private readonly IService<Car> _paymentService;
        private readonly ILogger _logger;

        private CancellationTokenSource _cts;
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
            IService<Car> refuelService,
            IService<Car> paymentService)
        {
            _config = config;
            _fuelTank = fuelTank;
            _refuelQueue = refuelQueue;
            _paymentQueue = paymentQueue;
            _refuellerPool = refuellerPool;
            _cashierPool = cashierPool;
            _carGenerator = carGenerator;
            _fuelTruckGenerator = fuelTruckGenerator;
            _refuelService = refuelService;
            _paymentService = paymentService;
            _logger = logger;

            SetupEvents();
        }

        private void SetupEvents()
        {
            _carGenerator.Generated += car =>
            {
                _refuelQueue.Enqueue(car);
                _logger.LogInfo($"Машина {car.Id} прибыла на заправку (нужно {car.RequiredFuel}л)");
            };

            _fuelTruckGenerator.Generated += truck =>
            {
                _fuelTank.Refuel(truck.FuelAmount);
                _logger.LogInfo($"Бензовоз {truck.Id} доставил {truck.FuelAmount}л. Теперь в резервуаре: {_fuelTank.CurrentLevel}л");
            };

            _refuelService.ItemProcessed += car =>
            {
                _paymentQueue.Enqueue(car);
                _logger.LogInfo($"Машина {car.Id} заправлена, переходит на оплату");
            };

            _paymentService.ItemProcessed += car =>
            {
                _logger.LogInfo($"Машина {car.Id} обслужена и уезжает");
            };

            _fuelTank.FuelLevelChanged += level =>
            {
                if (level < Constants.FuelThreshold)
                    _logger.LogWarning($"Низкий уровень топлива: {level}л");
            };
        }

        public async Task RunSimulation()
        {
            lock (_lock)
            {
                if (_isRunning)
                    throw new InvalidOperationException("Моделирование уже запущено");
                _isRunning = true;
                _cts = new CancellationTokenSource();
                _simulationStartTime = DateTime.Now;
            }

            try
            {
                var tasks = new List<Task>
                {
                    _carGenerator.StartGeneration(_cts.Token),          
                    _fuelTruckGenerator.StartGeneration(_cts.Token),

                    Task.Run(() => _refuelService.Process(_cts.Token)),
                    Task.Run(() => _paymentService.Process(_cts.Token)),

                    MonitorSimulation()
                };

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

                _logger.LogStatistics(_stats);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при сборе статистики: {ex.Message}");
            }
        }

        public async Task MonitorSimulation()
        {
            await Task.Delay(TimeSpan.FromSeconds(_config.SimulationDurationSeconds), _cts.Token);
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
            _cts.Cancel();
            _isRunning = false;
        }
    }
}