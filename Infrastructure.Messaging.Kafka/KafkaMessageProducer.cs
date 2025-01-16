using Confluent.Kafka;
using Infrastructure.Messaging.Kafka.Serialization;
using Microsoft.Extensions.Logging;
using SharedKernel.Messaging;
using System.Text.Json;

namespace Infrastructure.Messaging.Kafka
{
    public class KafkaMessageProducer<T> : IMessageProducer<T>, IDisposable where T : class
    {
        private readonly ILogger<IMessageProducer<T>> _logger;
        private readonly IProducer<string, T> _producer;
        private readonly string _topic;

        public KafkaMessageProducer(ILogger<IMessageProducer<T>> logger, IProducer<string, T> producer, string topic)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _producer = producer ?? throw new ArgumentNullException(nameof(producer));
            _topic = topic ?? throw new ArgumentNullException(nameof(topic));
        }

        public async Task PublishAsync(T message, string? key = null)
        {
            try
            {
                // Enviar el mensaje a Kafka
                var result = await _producer.ProduceAsync(
                    _topic,
                    new Message<string, T> { Key = key, Value = message }
                );

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
                _producer.Flush(TimeSpan.FromSeconds(5));
                _producer.Dispose();
                _logger.LogInformation("Kafka producer for topic '{Topic}' disposed successfully.", _topic);
            }
        }
    }
}
