using GasStation.FileOperations.Classes;

namespace GasStation.FileOperations.Interfaces
{
    public interface ILogger
    {
        void LogInfo(string message);
        void LogWarning(string message);
        void LogError(string message);
        void LogSimulationStart(SimulationConfig config);
        void LogStatistics(SimulationStats stats);
    }
}
