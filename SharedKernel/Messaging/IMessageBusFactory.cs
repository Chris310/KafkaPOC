
using Microsoft.Extensions.Logging;

namespace Infrastructure.Shared.Messaging
{
    public interface IMessageBusFactory
    {
        IMessageProducer<T> CreateProducer<T>(string topicOrQueueName, ILogger<IMessageProducer<T>> logger) where T : class;
        IMessageConsumer<T> CreateConsumer<T>(string topicOrQueueName, string? groupId, ILogger<IMessageConsumer<T>> logger) where T : class;
    }

}
