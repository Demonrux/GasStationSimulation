namespace GasStation.Services.Interfaces
{
    public interface IService<T>
    {
        Task Process(CancellationToken cancellationToken);
        event Action<T> ItemProcessed;
        int ProcessedCount { get; }
    }
}
