using GasStation.Core.Models;

namespace GasStation.Core.Workers
{
    public abstract class Worker
    {
        public int Id { get; }
        public string Type { get; }
        public bool IsBusy => CurrentCar != null;
        public Car CurrentCar { get; protected set; }

        private readonly object _counterLock = new object();

        private int _processedCount = 0;

        public int ProcessedCount
        {
            get { lock (_counterLock) return _processedCount; }
        }
        public event Action<int, int> WorkStarted;
        public event Action<int, int, TimeSpan> WorkCompleted;

        protected Worker(int id, string type)
        {
            Id = id;
            Type = type;
        }

        public async Task ProcessCar(Car car, TimeSpan processingTime, CancellationToken cancellationToken)
        {
            if (IsBusy)
                throw new InvalidOperationException($"{Type} {Id} уже занят");

            CurrentCar = car;

            try
            {
                WorkStarted?.Invoke(Id, car.Id);
                await Task.Delay(processingTime, cancellationToken);

                lock (_counterLock)
                {
                    _processedCount++;
                }

                WorkCompleted?.Invoke(Id, car.Id, processingTime);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            finally
            {
                CurrentCar = null;
            }
        }
    }
}