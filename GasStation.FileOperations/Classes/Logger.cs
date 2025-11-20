using GasStation.FileOperations.Interfaces;

namespace GasStation.FileOperations.Classes
{
    public class Logger : ILogger
    {
        private readonly string _logFilePath;
        private readonly object _lock = new();

        public Logger(string logFilePath)
        {
            _logFilePath = logFilePath;

            lock (_lock)
            {
                File.WriteAllText(_logFilePath, $"Моделирование АЗС {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n\n");
            }
        }

        public void LogInfo(string message)
        {
            WriteMessage($"[INFO] {message}");
        }

        public void LogWarning(string message)
        {
            WriteMessage($"[WARN] {message}");
        }

        public void LogError(string message)
        {
            WriteMessage($"[ERROR] {message}");
        }

        private void WriteMessage(string message)
        {
            var logEntry = $"{DateTime.Now:HH:mm:ss} {message}";

            Console.WriteLine(logEntry);

            lock (_lock)
            {
                File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
            }
        }

        public void LogSimulationStart(SimulationConfig config)
        {
            LogInfo("Запуск моделирования АЗС");
            LogInfo($"Длительность: {config.SimulationDurationSeconds} сек");
            LogInfo($"Работники: {config.RefuellerCount} заправщиков, {config.CashierCount} кассиров");
            LogInfo($"Топливо: {config.InitialFuelLevel}/{config.FuelTankCapacity}л");
            LogInfo("=" + new string('=', 50));
        }

        public void LogStatistics(SimulationStats stats)
        {
            var separator = new string('=', 60);

            Console.WriteLine(separator);
            Console.WriteLine("СТАТИСТИКА МОДЕЛИРОВАНИЯ");
            Console.WriteLine(separator);
            Console.WriteLine($"Длительность моделирования: {stats.SimulationDuration.TotalSeconds:F1} сек");
            Console.WriteLine($"Всего сгенерировано машин: {stats.TotalCarsGenerated}");
            Console.WriteLine($"Обслужено машин (заправка): {stats.TotalCarsRefueled}");
            Console.WriteLine($"Обслужено машин (оплата): {stats.TotalCarsPaid}");
            Console.WriteLine($"Всего бензовозов: {stats.TotalFuelTrucks}");
            Console.WriteLine($"Доставлено топлива: {stats.TotalFuelDelivered}л");
            Console.WriteLine($"Топлива в резервуаре: {stats.FinalFuelLevel}л");
            Console.WriteLine($"Максимальная очередь заправки: {stats.MaxQueueLengthRefuel} машин");
            Console.WriteLine($"Максимальная очередь оплаты: {stats.MaxQueueLengthPayment} машин");

            Console.WriteLine("Статистика заправщиков:");
            foreach (var stat in stats.RefuellerStats)
                Console.WriteLine($"  • Заправщик {stat.Key}: {stat.Value} машин");

            Console.WriteLine("Статистика кассиров:");
            foreach (var stat in stats.CashierStats)
                Console.WriteLine($"  • Кассир {stat.Key}: {stat.Value} машин");

            Console.WriteLine(separator);

            lock (_lock)
            {
                File.AppendAllText(_logFilePath, separator + Environment.NewLine);
                File.AppendAllText(_logFilePath, "СТАТИСТИКА МОДЕЛИРОВАНИЯ" + Environment.NewLine);
                File.AppendAllText(_logFilePath, separator + Environment.NewLine);
                File.AppendAllText(_logFilePath, $"Длительность моделирования: {stats.SimulationDuration.TotalSeconds:F1} сек" + Environment.NewLine);
                File.AppendAllText(_logFilePath, $"Всего сгенерировано машин: {stats.TotalCarsGenerated}" + Environment.NewLine);
                File.AppendAllText(_logFilePath, $"Обслужено машин (заправка): {stats.TotalCarsRefueled}" + Environment.NewLine);
                File.AppendAllText(_logFilePath, $"Обслужено машин (оплата): {stats.TotalCarsPaid}" + Environment.NewLine);
                File.AppendAllText(_logFilePath, $"Всего бензовозов: {stats.TotalFuelTrucks}" + Environment.NewLine);
                File.AppendAllText(_logFilePath, $"Доставлено топлива: {stats.TotalFuelDelivered}л" + Environment.NewLine);
                File.AppendAllText(_logFilePath, $"Топлива в резервуаре: {stats.FinalFuelLevel}л" + Environment.NewLine);
                File.AppendAllText(_logFilePath, $"Максимальная очередь заправки: {stats.MaxQueueLengthRefuel} машин" + Environment.NewLine);
                File.AppendAllText(_logFilePath, $"Максимальная очередь оплаты: {stats.MaxQueueLengthPayment} машин" + Environment.NewLine);

                File.AppendAllText(_logFilePath, "Статистика заправщиков:" + Environment.NewLine);
                foreach (var stat in stats.RefuellerStats)
                    File.AppendAllText(_logFilePath, $"  • Заправщик {stat.Key}: {stat.Value} машин" + Environment.NewLine);

                File.AppendAllText(_logFilePath, "Статистика кассиров:" + Environment.NewLine);
                foreach (var stat in stats.CashierStats)
                    File.AppendAllText(_logFilePath, $"  • Кассир {stat.Key}: {stat.Value} машин" + Environment.NewLine);

                File.AppendAllText(_logFilePath, separator + Environment.NewLine);
            }
        }
    }
}
