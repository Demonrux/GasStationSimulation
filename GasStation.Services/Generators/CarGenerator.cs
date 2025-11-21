using GasStation.Core.Models;
using GasStation.Core.Utils;
using GasStation.FileOperations.Interfaces;
using GasStation.Services.Interfaces;

namespace GasStation.Services.Generators
{
    public class CarGenerator : IGenerator<Car>
    {
        private int _generatedCount;
        private readonly TimeSpan _generationInterval;
        private readonly ILogger _logger;
        private int _carId;

        public int GeneratedCount => _generatedCount;
        public event Action<Car> Generated;

        public CarGenerator(TimeSpan generationInterval, ILogger logger)
        {
            _generationInterval = generationInterval;
            _logger = logger;
        }

        public async Task StartGeneration(CancellationToken cancellationToken)
        {
             await Task.Run(async () =>
             {
                 _logger.LogInfo("Car генератор запущен");
                 while (!cancellationToken.IsCancellationRequested)
                 {
                     try
                     {
                         var delay = TimingCalculator.CarGeneration(_generationInterval);
                         await Task.Delay(delay, cancellationToken);
        
                         var newCarId = Interlocked.Increment(ref _carId);
                         Interlocked.Increment(ref _generatedCount);
        
                         var car = new Car(newCarId);
                         _logger.LogInfo($"Машина {car.Id} сгенерирована (нужно {car.RequiredFuel}л)");
        
                         Generated?.Invoke(car);
                     }
                     catch (OperationCanceledException)
                     {
                         _logger.LogWarning("Генерация машин остановлена");
                         break;
                     }
                 }
             }, cancellationToken);
         }
    }
}
