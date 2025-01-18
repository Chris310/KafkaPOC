using Xunit;
using Moq;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Infrastructure.Messaging.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel.Configuration;
using Infrastructure.Shared.Messaging;

public class ProducerTests
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
                Url = "http://localhost:8081",
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
}
