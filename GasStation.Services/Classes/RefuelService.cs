using GasStation.Core.Enums;
using GasStation.Core.Models;
using GasStation.Core.Utils;
using GasStation.Core.Workers;
using GasStation.FileOperations.Interfaces;
using GasStation.Services.Interfaces;

namespace GasStation.Services.Classes
{
    public class RefuelService : IService<Car>
    {
        private readonly object _queueLock = new object();
        private readonly CarQueue _refuelQueue;
        private readonly WorkerPool<Refueller> _refuellerPool;
        private readonly FuelTank _fuelTank;
        private readonly ILogger _logger;
        private int _processedCount;

        public string ServiceType => "Refuel";
        public int ProcessedCount => _processedCount;
        public event Action<Car> ItemProcessed;

        public RefuelService(CarQueue refuelQueue, WorkerPool<Refueller> refuellerPool, FuelTank fuelTank, ILogger logger)
        {
            _refuelQueue = refuelQueue;
            _refuellerPool = refuellerPool;
            _fuelTank = fuelTank;
            _logger = logger;

            _refuellerPool.SubscribeToWorkEvents(
                onWorkStarted: (workerId, carId) => _logger.LogInfo($"Заправщик {workerId} начал заправку машины {carId}. Топливо: {_fuelTank.CurrentLevel}л"),
                onWorkCompleted: (workerId, carId, duration) =>
                {
                    _logger.LogInfo($"Заправщик {workerId} закончил заправку машины {carId} за {duration.TotalSeconds:F1}с");
                    Interlocked.Increment(ref _processedCount);
                }
            );
        }

        public async Task Process(CancellationToken cancellationToken)
        {
            _logger.LogInfo("Сервис заправки запущен");
            while (!cancellationToken.IsCancellationRequested)
            {
                Car car = null;

                lock (_queueLock)
                {
                    car = _refuelQueue.Dequeue();
                }

                if (car != null)
                {
                    _logger.LogInfo($"Обработка машины {car.Id} (нужно {car.RequiredFuel}л), в очереди: {_refuelQueue.Count} машин");

                    if (_fuelTank.TryReserveFuel(car.RequiredFuel))
                    {
                        var assigned = await _refuellerPool.TryAssignWork(car, refueller => !refueller.IsBusy, cancellationToken);

                        if (assigned)
                        {
                            _logger.LogInfo($"Машина {car.Id} заправлена и отправлена на оплату");
                            car.State = CarState.WaitingForPayment;
                            ItemProcessed?.Invoke(car);
                        }
                        else
                        {
                            lock (_queueLock)
                            {
                                _refuelQueue.Enqueue(car);
                                _logger.LogWarning($"Не удалось назначить заправщика для машины {car.Id}");
                            }
                        }
                    }
                    else
                    {
                        lock (_queueLock)
                        {
                            _refuelQueue.Enqueue(car);
                            _logger.LogInfo($"Машина {car.Id} возвращена в очередь заправки - недостаточно топлива ({_fuelTank.CurrentLevel}л)");
                        }
                        await Task.Delay(TimingCalculator.RetryDelay, cancellationToken);
                    }
                }
                else
                {
                    await Task.Delay(TimingCalculator.RetryDelay, cancellationToken);
                }
            }
        }
    }
}