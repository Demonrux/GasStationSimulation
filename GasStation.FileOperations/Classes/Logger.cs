using GasStation.FileOperations.Interfaces;
using System.Text;

namespace GasStation.FileOperations.Classes
{
    public class Logger : ILogger, IDisposable
    {
        private readonly string _logFilePath;
        private readonly StreamWriter _writer;
        private readonly object _lock = new();

        public Logger(string logFilePath)
        {
            _logFilePath = logFilePath;

            _writer = new StreamWriter(_logFilePath, append: false, Encoding.UTF8)
            {
                AutoFlush = true 
            };

            lock (_lock)
            {
                _writer.WriteLine($"Моделирование АЗС {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                _writer.WriteLine(new string('=', 50));
                _writer.WriteLine();
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
                _writer.WriteLine(logEntry);
            }
        }

        public void LogSimulationStart(SimulationConfig config)
        {
            LogInfo("Запуск моделирования АЗС");
            LogInfo($"Длительность: {config.SimulationDurationSeconds} сек");
            LogInfo($"Работники: {config.RefuellerCount} заправщиков, {config.CashierCount} кассиров");
            LogInfo($"Топливо: {config.InitialFuelLevel}/{config.FuelTankCapacity}л");
            LogInfo($"Начальный баланс: {config.InitialBalance} RUB");
            LogInfo(new string('=', 50));
        }

        public void LogStatistics(SimulationStats stats)
        {
            var separator = new string('=', 60);

            lock (_lock)
            {
                _writer.WriteLine(separator);
                _writer.WriteLine("Статистика моделирования");
                _writer.WriteLine(separator);
                _writer.WriteLine($"Длительность моделирования: {stats.SimulationDuration.TotalSeconds:F1} сек");
                _writer.WriteLine($"Баланс: {stats.EconomyStats.Balance} RUB");
                _writer.WriteLine($"Выручка: {stats.EconomyStats.TotalRevenue} RUB");
                _writer.WriteLine($"Расходы: {stats.EconomyStats.TotalExpenses} RUB");
                _writer.WriteLine($"Прибыль: {stats.EconomyStats.Profit} RUB");
                _writer.WriteLine($"Всего сгенерировано машин: {stats.TotalCarsGenerated}");
                _writer.WriteLine($"Обслужено машин (заправка): {stats.TotalCarsRefueled}");
                _writer.WriteLine($"Обслужено машин (оплата): {stats.TotalCarsPaid}");
                _writer.WriteLine($"Всего бензовозов: {stats.TotalFuelTrucks}");
                _writer.WriteLine($"Доставлено топлива: {stats.TotalFuelDelivered}л");
                _writer.WriteLine($"Топлива в резервуаре: {stats.FinalFuelLevel}л");
                _writer.WriteLine($"Максимальная очередь заправки: {stats.MaxQueueLengthRefuel} машин");
                _writer.WriteLine($"Максимальная очередь оплаты: {stats.MaxQueueLengthPayment} машин");

                _writer.WriteLine("Статистика заправщиков:");
                foreach (var stat in stats.RefuellerStats)
                {
                    var salary = stats.GetRefuellerSalary(stat.Value);
                    _writer.WriteLine($"  • Заправщик {stat.Key}: {stat.Value} машин, зарплата: {salary} руб.");
                }

                _writer.WriteLine("Статистика кассиров:");
                foreach (var stat in stats.CashierStats)
                {
                    var salary = stats.GetCashierSalary(stat.Value);
                    _writer.WriteLine($"  • Кассир {stat.Key}: {stat.Value} машин, зарплата: {salary} руб.");
                }

                _writer.WriteLine(separator);
                _writer.WriteLine(); 
            }

            Console.WriteLine(separator);
            Console.WriteLine("Статистика моделирования");
            Console.WriteLine(separator);
            Console.WriteLine($"Длительность моделирования: {stats.SimulationDuration.TotalSeconds:F1} сек");
            Console.WriteLine($"Баланс: {stats.EconomyStats.Balance} RUB");
            Console.WriteLine($"Выручка: {stats.EconomyStats.TotalRevenue} RUB");
            Console .WriteLine($"Расходы: {stats.EconomyStats.TotalExpenses} RUB");
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
            Console.WriteLine();
        }

        public void Dispose()
        {
            lock (_lock)
            {
                _writer?.Dispose();
            }
        }
    }
}
