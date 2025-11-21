using System.Globalization;
using GasStation.FileOperations.Interfaces;

namespace GasStation.FileOperations.Classes
{
    public class CsvConfigReader : IConfigReader
    {
        public SimulationConfig ReadConfig(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Конфигурационный файл не найден: {filePath}");

            var config = new SimulationConfig();
            var lines = File.ReadAllLines(filePath);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                var parts = line.Split(',');
                if (parts.Length != 2) continue;

                var key = parts[0].Trim();
                var value = parts[1].Trim();
                SetConfigValue(config, key, value);
            }

            ValidateConfig(config);
            return config;
        }

        private void SetConfigValue(SimulationConfig config, string key, string value)
        {
            try
            {
                switch (key)
                {
                    case "SimulationDurationSeconds":
                        config.SimulationDurationSeconds = int.Parse(value, CultureInfo.InvariantCulture);
                        break;
                    case "CarGenerationInterval":
                        config.CarGenerationInterval = int.Parse(value, CultureInfo.InvariantCulture);
                        break;
                    case "FuelTruckGenerationInterval":
                        config.FuelTruckGenerationInterval = int.Parse(value, CultureInfo.InvariantCulture);
                        break;
                    case "RefuellerCount":
                        config.RefuellerCount = int.Parse(value, CultureInfo.InvariantCulture);
                        break;
                    case "CashierCount":
                        config.CashierCount = int.Parse(value, CultureInfo.InvariantCulture);
                        break;
                    case "FuelTankCapacity":
                        config.FuelTankCapacity = int.Parse(value, CultureInfo.InvariantCulture);
                        break;
                    case "InitialFuelLevel":
                        config.InitialFuelLevel = int.Parse(value, CultureInfo.InvariantCulture);
                        break;
                    case "FuelPurchasePrice":
                        config.FuelPurchasePrice = decimal.Parse(value, CultureInfo.InvariantCulture);
                        break;
                    case "FuelSellPrice":
                        config.FuelSellPrice = decimal.Parse(value, CultureInfo.InvariantCulture);
                        break;
                    case "RefuellerSalaryPerCar":
                        config.RefuellerSalaryPerCar = decimal.Parse(value, CultureInfo.InvariantCulture);
                        break;
                    case "CashierSalaryPerCar":
                        config.CashierSalaryPerCar = decimal.Parse(value, CultureInfo.InvariantCulture);
                        break;
                    case "InitialBalance":
                        config.InitialBalance = decimal.Parse(value, CultureInfo.InvariantCulture);
                        break;

                    default:
                        Console.WriteLine($"Неизвестный параметр конфигурации: {key}");
                        break;
                }
            }
            catch (FormatException ex)
            {
                throw new FormatException($"Неверный формат значения для параметра '{key}': '{value}'", ex);
            }
        }

        private void ValidateConfig(SimulationConfig config)
        {
            var errors = new List<string>();

            if (config.SimulationDurationSeconds <= 0)
                errors.Add("Длительность симуляции должна быть положительной");
            if (config.CarGenerationInterval <= 0)
                errors.Add("Интервал генерации машин должен быть положительным");
            if (config.FuelTruckGenerationInterval <= 0)
                errors.Add("Интервал генерации бензовозов должен быть положительным");
            if (config.RefuellerCount <= 0)
                errors.Add("Количество заправщиков должно быть положительным");
            if (config.CashierCount <= 0)
                errors.Add("Количество кассиров должно быть положительным");
            if (config.FuelTankCapacity <= 0)
                errors.Add("Емкость резервуара должна быть положительной");
            if (config.InitialFuelLevel < 0)
                errors.Add("Начальный уровень топлива не может быть отрицательным");
            if (config.InitialFuelLevel > config.FuelTankCapacity)
                errors.Add("Начальный уровень топлива не может превышать емкость резервуара");

            if (config.FuelPurchasePrice <= 0)
                errors.Add("Цена закупки топлива должна быть положительной");
            if (config.FuelSellPrice <= 0)
                errors.Add("Цена продажи топлива должна быть положительной");
            if (config.FuelSellPrice <= config.FuelPurchasePrice)
                errors.Add("Цена продажи должна быть выше цены закупки для рентабельности");
            if (config.RefuellerSalaryPerCar < 0)
                errors.Add("Зарплата заправщика не может быть отрицательной");
            if (config.CashierSalaryPerCar < 0)
                errors.Add("Зарплата кассира не может быть отрицательной");
            if (config.InitialBalance < 0)
                errors.Add("Начальный баланс не может быть отрицательным");

            if (errors.Count > 0)
            {
                throw new InvalidOperationException($"Ошибки конфигурации:\n{string.Join("\n", errors)}");
            }
        }
    }
}