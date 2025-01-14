
namespace SharedKernel.Messaging
{
    public interface IMessageHandler<T>
    {
        /// <summary>
        /// Handles a message of type T.
        /// </summary>
        /// <param name="message">The message to handle.</param>
        Task HandleMessageAsync(T message);
    }

    public interface IBatchMessageHandler<T>
    {
        /// <summary>
        /// Handles a batch of messages of type T.
        /// </summary>
        /// <param name="messages">The batch of messages to handle.</param>
        Task HandleBatchAsync(IEnumerable<T> messages);
    }
}
