
namespace SharedKernel.Messaging
{
    public interface IMessageConsumer<T> : IDisposable
    {
        T Consume();
        IEnumerable<T> ConsumeBatch(int batchSize, TimeSpan timeout);
        void Commit();
    }
}
