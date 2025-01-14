using Confluent.Kafka;
using Infrastructure.Messaging.Kafka.Serialization;
using Microsoft.Extensions.Logging;
using SharedKernel.Messaging;
using System.Text.Json;

namespace Infrastructure.Messaging.Kafka
{
    public class KafkaMessageProducer<T> : IMessageProducer<T>, IDisposable
    {
        private readonly ILogger<IMessageProducer<T>> _logger;
        private readonly IMessageSerializer _serializer;
        private readonly IProducer<string, string> _producer;
        private readonly string _topic;

        public KafkaMessageProducer(ILogger<IMessageProducer<T>> logger, IMessageSerializer serializer, ProducerConfig config, string topic)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _topic = topic ?? throw new ArgumentNullException(nameof(topic));

            if (config == null)
                throw new ArgumentNullException(nameof(config));

            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        public async Task PublishAsync(T message, string key = null)
        {
            var jsonMessage = _serializer.Serialize(message);

            try
            {
                // Enviar el mensaje a Kafka
                await _producer.ProduceAsync(_topic, new Message<string, string>
                {
                    Key = key,
                    Value = jsonMessage
                });

                _logger.LogInformation("Message delivered to topic '{Topic}' successfully.", _topic);
            }
            catch (ProduceException<Null, string> ex)
            {
                _logger.LogError(ex, "Failed to deliver message to topic '{Topic}'. Error: {Reason}", _topic, ex.Error.Reason);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while delivering message to topic '{Topic}'.", _topic);
            }
        }

        public void Dispose()
        {
            try
            {
                _producer.Flush(TimeSpan.FromSeconds(10));
                _logger.LogInformation("Kafka producer for topic '{Topic}' flushed successfully.", _topic);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while flushing the Kafka producer for topic '{Topic}'.", _topic);
            }
            finally
            {
                _producer.Dispose();
                _logger.LogInformation("Kafka producer for topic '{Topic}' disposed successfully.", _topic);
            }
        }
    }
}
