using System.Collections.Concurrent;

namespace GasStation.Core.Models
{
    public class CarQueue
    {
        private readonly ConcurrentQueue<Car> _queue = new();
        private int _maxQueueLength;

        public int Count => _queue.Count;
        public int MaxQueueLength => _maxQueueLength;

        public void Enqueue(Car car)
        {
            _queue.Enqueue(car);

            var currentCount = _queue.Count;
            if (currentCount > _maxQueueLength)
                Interlocked.Exchange(ref _maxQueueLength, currentCount);
        }

        public Car Dequeue() => _queue.TryDequeue(out var car) ? car : null;
    }
}