namespace Infrastructure.Shared.Messaging
{
    public interface IMessageProducer<T> : IDisposable where T : class
    {
        Task PublishAsync(T message, string? key = null);
    }
}
