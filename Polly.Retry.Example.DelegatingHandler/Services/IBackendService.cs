namespace Polly.Retry.Example.DelegatingHandler.Services
{
    public interface IBackendService
    {
        Task<string> GetDataAsync();
    }
}
