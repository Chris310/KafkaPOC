
namespace SharedKernel.Messaging
{
    public interface IMessageConsumer<T> : IDisposable
    {
        T Consume();
        T Consume(TimeSpan timeout);
        void Commit();
    }
}
