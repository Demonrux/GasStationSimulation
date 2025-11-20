using GasStation.Core.Models;

namespace GasStation.Core.Utils
{
    public static class TimingCalculator
    {
        public static readonly TimeSpan RetryDelay = TimeSpan.FromMicroseconds(400);

        public static TimeSpan CarGeneration(TimeSpan baseInterval)
        {
            var factor = Constants.TimingСoefficient + Random.Shared.NextDouble(); 
            return TimeSpan.FromMilliseconds(baseInterval.TotalMilliseconds * factor);
        }

        public static TimeSpan Refueling(int fuelAmount)
        {
            var baseTime = Constants.BaseRefuelingTime + (fuelAmount * Constants.FuelCoefficient);
            var randomTime = baseTime * (Constants.TimingСoefficient + Random.Shared.NextDouble());
            return TimeSpan.FromSeconds(randomTime);
        }

        public static TimeSpan Payment()
        {
            return TimeSpan.FromSeconds(Constants.BasePaymentTime + (Constants.TimingСoefficient + Random.Shared.NextDouble()));
        }

    }
}