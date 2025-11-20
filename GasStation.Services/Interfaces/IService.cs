namespace GasStation.Services.Interfaces
{
    public interface IService<T>
    {
        Task Process(CancellationToken cancellationToken);
        event Action<T> ItemProcessed;
        string ServiceType { get; }
        int ProcessedCount { get; }
    }
}
