namespace GasStation.Services.Interfaces
{
    public interface IGenerator<T>
    {
        Task StartGeneration(CancellationToken cancellationToken);
        event Action<T> Generated;
        int GeneratedCount { get; }
    }
}
