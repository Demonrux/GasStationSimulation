using GasStation.Engine.Classes;
using GasStation.Engine.Interfaces;
using GasStation.FileOperations.Classes;
using GasStation.FileOperations.Interfaces;

namespace GasStation
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                IConfigReader configReader = new CsvConfigReader();
                string configPath = "C:\\Users\\Пользователь\\source\\repos\\GasStationModel\\GasStation.FileOperations\\files\\config.csv";

                if (!File.Exists(configPath))
                {
                    Console.WriteLine("Конфигурационный файл не найден!");
                    return;
                }

                Console.WriteLine($"Чтение конфигурации из: {configPath}");
                SimulationConfig config = configReader.ReadConfig(configPath);

                ILogger logger = new Logger("C:\\Users\\Пользователь\\source\\repos\\GasStationModel\\GasStation.FileOperations\\files\\log.txt");

                IEngine engine = GasStationFactory.Create(config, logger);

                logger.LogSimulationStart(config);

                Task simulationTask = engine.RunSimulation();
                await Task.WhenAny(simulationTask);

            }
            catch (Exception exception)
            {
                Console.WriteLine($"ОШИБКА: {exception.Message}");
            }

            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
        }

    }
}