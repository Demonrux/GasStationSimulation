using GasStation.FileOperations.Classes;

namespace GasStation.FileOperations.Interfaces
{
    public interface IConfigReader
    {
        SimulationConfig ReadConfig(string filePath);
    }
}