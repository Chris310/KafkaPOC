using Confluent.Kafka;
using Infrastructure.Messaging.Kafka.Serialization;
using Microsoft.Extensions.Logging;
using SharedKernel.Messaging;
using System.Text.Json;

namespace Infrastructure.Messaging.Kafka
{
    public class KafkaMessageConsumer<T> : IMessageConsumer<T>, IDisposable
    {
        private readonly ILogger<IMessageConsumer<T>> _logger;
        private readonly IMessageSerializer _serializer;
        private readonly IConsumer<string, string> _consumer;
        private readonly string _topic;

        public KafkaMessageConsumer(ILogger<IMessageConsumer<T>> logger, IMessageSerializer serializer, ConsumerConfig config, string topic)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _topic = topic ?? throw new ArgumentNullException(nameof(topic));

            if (config == null)
                throw new ArgumentNullException(nameof(config));

            _consumer = new ConsumerBuilder<string, string>(config).Build();
            _consumer.Subscribe(_topic);

            _logger.LogInformation("Kafka consumer created for topic: {Topic}", _topic);
        }

        public T Consume()
        {
            try
            {
                var consumeResult = _consumer.Consume();
                _logger.LogInformation("Message consumed from topic '{Topic}': {Message}", _topic, consumeResult.Message.Value);

                return _serializer.Deserialize<T>(consumeResult.Message.Value);
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

        public IEnumerable<T> ConsumeBatch(int batchSize, TimeSpan timeout)
        {
            var messages = new List<T>();
            var startTime = DateTime.UtcNow;

            try
            {
                while (messages.Count < batchSize && (DateTime.UtcNow - startTime) < timeout)
                {
                    try
                    {
                        var consumeResult = _consumer.Consume(timeout);

                        if (consumeResult != null)
                        {
                            var deserializedMessage = _serializer.Deserialize<T>(consumeResult.Message.Value);
                            messages.Add(deserializedMessage);
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(ex, "Failed to consume a message from topic '{Topic}': {Reason}", ex.ConsumerRecord?.Topic, ex.Error.Reason);
                        throw;
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Failed to deserialize a message.");
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while consuming a batch of messages.");
                throw;
            }

            _logger.LogInformation("Successfully consumed {Count} messages in batch from topic '{Topic}'.", messages.Count, _consumer.Subscription.FirstOrDefault());

            return messages;
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
