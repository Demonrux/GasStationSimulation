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
            LogInfo($"Начальный баланс:{config.InitialBalance} RUB");
            LogInfo("=" + new string('=', 50));
        }

        public void LogStatistics(SimulationStats stats)
        {
            var separator = new string('=', 60);

            Console.WriteLine(separator);
            Console.WriteLine("Статистика моделирования");
            Console.WriteLine(separator);
            Console.WriteLine($"Длительность моделирования: {stats.SimulationDuration.TotalSeconds:F1} сек");
            Console.WriteLine($"Баланс: {stats.EconomyStats.Balance} RUB");
            Console.WriteLine($"Выручка: {stats.EconomyStats.TotalRevenue} RUB");
            Console.WriteLine($"Расходы: {stats.EconomyStats.TotalExpenses} RUB");
            Console.WriteLine($"Прибыль: {stats.EconomyStats.Profit} RUB");
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
            {
                var salary = stats.GetRefuellerSalary(stat.Value);
                Console.WriteLine($"  • Заправщик {stat.Key}: {stat.Value} машин, зарплата: {salary} руб.");
            }

            Console.WriteLine("Статистика кассиров:");
            foreach (var stat in stats.CashierStats)
            {
                var salary = stats.GetCashierSalary(stat.Value);
                Console.WriteLine($"  • Кассир {stat.Key}: {stat.Value} машин, зарплата: {salary} руб.");
            }

            Console.WriteLine(separator);

            lock (_lock)
            {
                File.AppendAllText(_logFilePath, separator + Environment.NewLine);
                File.AppendAllText(_logFilePath, "Статистика моделирования" + Environment.NewLine);
                File.AppendAllText(_logFilePath, separator + Environment.NewLine);
                File.AppendAllText(_logFilePath, $"Длительность моделирования: {stats.SimulationDuration.TotalSeconds} сек" + Environment.NewLine);
                File.AppendAllText(_logFilePath, $"Баланс: {stats.EconomyStats.Balance} RUB" + Environment.NewLine);
                File.AppendAllText(_logFilePath, $"Выручка: {stats.EconomyStats.TotalRevenue} RUB" + Environment.NewLine);
                File.AppendAllText(_logFilePath, $"Расходы: {stats.EconomyStats.TotalExpenses} RUB" + Environment.NewLine);
                File.AppendAllText(_logFilePath, $"Прибыль: {stats.EconomyStats.Profit} RUB" + Environment.NewLine);

                File.AppendAllText(_logFilePath, "Статистика заправщиков:" + Environment.NewLine);
                foreach (var stat in stats.RefuellerStats)
                {
                    var salary = stats.GetRefuellerSalary(stat.Value);
                    File.AppendAllText(_logFilePath, $"  • Заправщик {stat.Key}: {stat.Value} машин, зарплата: {salary} руб." + Environment.NewLine);
                }

                File.AppendAllText(_logFilePath, "Статистика кассиров:" + Environment.NewLine);
                foreach (var stat in stats.CashierStats)
                {
                    var salary = stats.GetCashierSalary(stat.Value);
                    File.AppendAllText(_logFilePath, $"  • Кассир {stat.Key}: {stat.Value} машин, зарплата: {salary} руб." + Environment.NewLine);
                }
            }
        }
    }
}
