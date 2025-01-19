using Moq;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Infrastructure.Messaging.Kafka;
using Infrastructure.Shared.Messaging;

namespace KafkaPOC.Tests.Producers
{
    public class ProducerTests
    {
        [Fact]
        public async Task PublishAsync_ShouldPublishMessage()
        {
            // Arrange
            var mockProducer = new Mock<IProducer<string, string>>();
            var mockLogger = new Mock<ILogger<IMessageProducer<string>>>();
            var producer = new KafkaMessageProducer<string>(mockLogger.Object, mockProducer.Object, "test-topic");

            // Act
            await producer.PublishAsync("test-message");

            // Assert
            mockProducer.Verify(p => p.ProduceAsync(
                "test-topic",
                It.Is<Message<string, string>>(m => m != null && m.Value == "test-message"),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
