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
                    await ProcessPayment(car, cancellationToken);
                }
                else
                {
                    await Task.Delay(TimingCalculator.RetryDelay, cancellationToken);
                }
            }
        }

        private async Task ProcessPayment(Car car, CancellationToken cancellationToken)
        {
            try
            {
                var assigned = await _cashierPool.TryAssignWork(car, cashier => true, cancellationToken);

                if (assigned)
                {
                    ItemProcessed?.Invoke(car);
                }
                else
                {
                    _paymentQueue.Enqueue(car);
                    _logger.LogInfo($"Машина {car.Id} возвращена в очередь оплаты - нет свободных кассиров");
                    await Task.Delay(TimingCalculator.RetryDelay, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Оплата остановлена");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при оплате машины {car.Id}: {ex.Message}");
                _paymentQueue.Enqueue(car);
            }
        }
    }
}
