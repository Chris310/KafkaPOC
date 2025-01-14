
namespace SharedKernel.Messaging
{
    public interface IMessageConsumer<T> : IDisposable
    {
        T Consume();
        void Commit();
    }
}
