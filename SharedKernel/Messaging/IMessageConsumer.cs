
namespace SharedKernel.Messaging
{
    public interface IMessageConsumer<T> : IDisposable where T : class
    {
        T? Consume();
        T? Consume(TimeSpan timeout);
        void Commit();
    }
}
