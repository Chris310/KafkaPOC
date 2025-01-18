using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel.Configuration;
using Infrastructure.Messaging.Kafka;
using Infrastructure.Shared.Messaging;

namespace KafkaPOC.Tests.Factories
{
    public class KafkaFactoryTests
    {
        [Fact]
        public void CreateProducer_ShouldReturnValidProducer()
        {
            // Arrange
            var mockLoggerFactory = new Mock<ILogger<KafkaFactory>>();
            var mockLoggerProducer = new Mock<ILogger<IMessageProducer<string>>>();
            var options = Options.Create(new MessagingConfiguration
            {
                BootstrapServers = "localhost:9092",
                SchemaRegistry = new SchemaRegistryConfiguration
                {
                    Url = "http://localhost:8081", // Asegúrate de incluir la URL del Schema Registry
                    ApiKey = "test",
                    ApiSecret = "test"
                },
                Topics = new Dictionary<string, TopicConfiguration>
        {
            { "test-topic", new TopicConfiguration { Name = "test-topic" } }
        }
            });

            var factory = new KafkaFactory(options, mockLoggerFactory.Object);

            // Act
            var producer = factory.CreateProducer<string>("test-topic", mockLoggerProducer.Object);

            // Assert
            Assert.NotNull(producer);
        }

        [Fact]
        public void CreateConsumer_ShouldReturnValidConsumer()
        {
            // Arrange
            var mockLoggerFactory = new Mock<ILogger<KafkaFactory>>();
            var mockLoggerConsumer = new Mock<ILogger<IMessageConsumer<string>>>();
            var options = Options.Create(new MessagingConfiguration
            {
                BootstrapServers = "localhost:9092",
                SchemaRegistry = new SchemaRegistryConfiguration
                {
                    Url = "http://localhost:8081", // Asegúrate de incluir la URL del Schema Registry
                    ApiKey = "test",
                    ApiSecret = "test"
                },
                Topics = new Dictionary<string, TopicConfiguration>
        {
            { "test-topic", new TopicConfiguration { Name = "test-topic" } }
        }
            });

            var factory = new KafkaFactory(options, mockLoggerFactory.Object);

            // Act
            var consumer = factory.CreateConsumer<string>("test-topic", "test-group", mockLoggerConsumer.Object);

            // Assert
            Assert.NotNull(consumer);
        }
    }
}
