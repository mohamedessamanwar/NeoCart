namespace NeoCommerce.Application.Contracts.Services
{
    public interface IEventPublisher
    {
        Task EnqueueAsync(string eventType, object payload);
    }
}
