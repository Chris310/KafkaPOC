namespace SharedKernel.Messaging
{
    public interface IMessageProducer<T> : IDisposable
    {
        Task PublishAsync(T message, string key = null);
    }
}
