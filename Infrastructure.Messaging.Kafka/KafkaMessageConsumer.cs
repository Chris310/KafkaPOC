using Confluent.Kafka;
using Infrastructure.Messaging.Kafka.Serialization;
using Microsoft.Extensions.Logging;
using SharedKernel.Messaging;
using System.Text.Json;

namespace Infrastructure.Messaging.Kafka
{
    public class KafkaMessageConsumer<T> : IMessageConsumer<T>, IDisposable where T : class
    {
        private readonly ILogger<IMessageConsumer<T>> _logger;
        private readonly IConsumer<string, T> _consumer;
        private readonly string _topic;

        public KafkaMessageConsumer(ILogger<IMessageConsumer<T>> logger, IConsumer<string, T> consumer, string topic)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
            _topic = topic ?? throw new ArgumentNullException(nameof(topic));

            _consumer.Subscribe(_topic);

            _logger.LogInformation("Kafka consumer created for topic: {Topic}", _topic);
        }

        public T? Consume(TimeSpan timeout)
        {
            try
            {
                var consumeResult = _consumer.Consume(timeout);

                if (consumeResult == null)
                {
                    _logger.LogWarning("No message was consumed from topic '{Topic}' within the timeout period.", _topic);
                    return default;
                }

                _logger.LogInformation("Message consumed from topic '{Topic}': {Message}", _topic, consumeResult.Message.Value);

                return consumeResult.Message.Value;
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Failed to consume message from topic '{Topic}'. Reason: {Reason}", _topic, ex.Error.Reason);
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize message from topic '{Topic}'.", _topic);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while consuming a message from topic '{Topic}'.", _topic);
                throw;
            }
        }

        public T? Consume()
        {
            try
            {
                var consumeResult = _consumer.Consume();

                if (consumeResult == null)
                {
                    _logger.LogWarning("No message was consumed from topic '{Topic}' within the timeout period.", _topic);
                    return default;
                }

                _logger.LogInformation("Message consumed from topic '{Topic}': {Message}", _topic, consumeResult.Message.Value);

                return consumeResult.Message.Value;
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Failed to consume message from topic '{Topic}'. Reason: {Reason}", _topic, ex.Error.Reason);
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize message from topic '{Topic}'.", _topic);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while consuming a message from topic '{Topic}'.", _topic);
                throw;
            }
        }

        public void Commit()
        {
            try
            {
                _consumer.Commit();
                _logger.LogInformation("Offsets committed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while committing offsets.");
                throw;
            }
        }

        public void Dispose()
        {
            try
            {
                _consumer.Close();
                _logger.LogInformation("Kafka consumer for topic '{Topic}' closed gracefully.", _topic);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while closing the Kafka consumer for topic '{Topic}'.", _topic);
            }
            finally
            {
                _consumer.Dispose();
            }
        }
    }
}
