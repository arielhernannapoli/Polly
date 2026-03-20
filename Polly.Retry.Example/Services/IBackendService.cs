namespace Polly.Retry.Example.Services
{
    public interface IBackendService
    {
        Task<string> GetDataAsync();
    }
}
