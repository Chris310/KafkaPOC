
using Microsoft.Extensions.Logging;

namespace SharedKernel.Messaging
{
    public interface IMessageBusFactory
    {
        IMessageProducer<T> CreateProducer<T>(string topicOrQueueName, ILogger<IMessageProducer<T>> logger);
        IMessageConsumer<T> CreateConsumer<T>(string topicOrQueueName, string? groupId, ILogger<IMessageConsumer<T>> logger);
    }

}
