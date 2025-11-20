using GasStation.Core.Models;
using GasStation.Core.Utils;

namespace GasStation.Core.Workers
{
    public class WorkerPool<T> where T : Worker
    {
        private readonly List<T> _workers;
        private int _lastAssignedIndex = -1;

        public int BusyCount => _workers.Count(workers => workers.IsBusy);
        public int FreeCount => _workers.Count - BusyCount;

        public WorkerPool(IEnumerable<T> workers)
        {
            _workers = workers.ToList();
        }

        public IEnumerable<T> GetWorkers()
        {
            return _workers.AsReadOnly();
        }
        public void SubscribeToWorkEvents(Action<int, int> onWorkStarted, Action<int, int, TimeSpan> onWorkCompleted)
        {
            foreach (var worker in _workers)
            {
                worker.WorkStarted += onWorkStarted;
                worker.WorkCompleted += onWorkCompleted;
            }
        }

        public async Task<bool> TryAssignWork(Car car, Func<T, bool> canProcess, CancellationToken cancellationToken)
        {
            var availableWorkers = _workers.Where(workers => !workers.IsBusy && canProcess(workers)).ToList();

            if (availableWorkers.Count == 0)
                return false;

            _lastAssignedIndex = (_lastAssignedIndex + 1) % availableWorkers.Count;
            var worker = availableWorkers[_lastAssignedIndex];

            TimeSpan processingTime = CalculateProcessingTime(worker, car);

            try
            {
                await worker.ProcessCar(car, processingTime, cancellationToken);
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception exception)
            {
                return false;
            }
        }

        private TimeSpan CalculateProcessingTime(Worker worker, Car car)
        {
            return worker switch
            {
                Refueller => TimingCalculator.Refueling(car.RequiredFuel),
                Cashier => TimingCalculator.Payment(),
                _ => TimeSpan.FromSeconds(2)
            };
        }
    }
}