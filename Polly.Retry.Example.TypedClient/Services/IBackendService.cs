namespace Polly.Retry.Example.TypedClient.Services
{
    public interface IBackendService
    {
        Task<string> GetDataAsync();
    }
}
