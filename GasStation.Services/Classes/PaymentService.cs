using GasStation.Core.Models;
using GasStation.Core.Workers;
using GasStation.Core.Utils;
using GasStation.Core.Enums;
using GasStation.FileOperations.Interfaces;
using GasStation.Services.Interfaces;

namespace GasStation.Services
{
    public class PaymentService : IService<Car>
    {
        private readonly CarQueue _paymentQueue;
        private readonly WorkerPool<Cashier> _cashierPool;
        private readonly ILogger _logger;
        private int _processedCount;

        public int ProcessedCount => _processedCount;
        public event Action<Car> ItemProcessed;

        public PaymentService(CarQueue paymentQueue, WorkerPool<Cashier> cashierPool, ILogger logger)
        {
            _paymentQueue = paymentQueue;
            _cashierPool = cashierPool;
            _logger = logger;
        }

        public async Task Process(CancellationToken cancellationToken)
        {
            _logger.LogInfo("Сервис оплаты запущен");
            while (!cancellationToken.IsCancellationRequested)
            {
                var car = _paymentQueue.Dequeue();

                if (car != null)
                {
                    _logger.LogInfo($"Обработка оплаты машины {car.Id}, в очереди: {_paymentQueue.Count} машин");

                    try
                    {
                        car.State = CarState.Paying;

                        var assigned = await _cashierPool.TryAssignWork(
                            car,
                            cashier => true,
                            cancellationToken
                        );

                        if (assigned)
                        {
                            car.State = CarState.Completed;
                            ItemProcessed?.Invoke(car);
                        }
                        else
                        {
                            car.State = CarState.WaitingForPayment;
                            _paymentQueue.Enqueue(car);
                            _logger.LogInfo($"Машина {car.Id} возвращена в очередь оплаты - нет свободных кассиров");
                            await Task.Delay(TimingCalculator.RetryDelay, cancellationToken);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogWarning("Оплата остановлена");
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Ошибка при оплате машины {car.Id}: {ex.Message}");
                        car.State = CarState.WaitingForPayment;
                        _paymentQueue.Enqueue(car);
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
