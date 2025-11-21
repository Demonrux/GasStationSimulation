using GasStation.Core.Models;
using GasStation.FileOperations.Interfaces;
using GasStation.Services.Interfaces;

namespace GasStation.Services.Generators
{
    public class FuelTruckGenerator : IGenerator<FuelTruck>
    {
        private readonly TimeSpan _generationInterval;
        private readonly ILogger _logger;

        private int _truckId;
        private int _generatedCount;

        public int GeneratedCount => _generatedCount;
        public event Action<FuelTruck> Generated;

        public FuelTruckGenerator(TimeSpan generationInterval, ILogger logger)
        {
            _generationInterval = generationInterval;
            _logger = logger;
        }

        public async Task StartGeneration(CancellationToken cancellationToken)
        {
            await Task.Run(async () =>
            {
                _logger.LogInfo("FuelTruck генератор запущен");
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(_generationInterval, cancellationToken);

                        var truck = new FuelTruck(++_truckId, Constants.FuelTruckAmount);
                        _generatedCount++;

                        _logger.LogInfo($"Бензовоз {truck.Id} сгенерирован (топливо {truck.FuelAmount}л)");

                        Generated?.Invoke(truck);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogWarning("Генерация бензовозов остановлена");
                        break;
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError($"Ошибка генерации: {exception.Message}");
                        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                    }
                }
            }, cancellationToken);
        }
    }
}
