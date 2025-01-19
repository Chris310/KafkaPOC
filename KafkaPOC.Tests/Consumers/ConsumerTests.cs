using Moq;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Infrastructure.Messaging.Kafka;
using Infrastructure.Shared.Messaging;

namespace KafkaPOC.Tests.Consumers
{
    public class ConsumerTests
    {
        [Fact]
        public void Consume_ShouldReturnMessage()
        {
            // Arrange
            var mockConsumer = new Mock<IConsumer<string, string>>();
            var mockLogger = new Mock<ILogger<IMessageConsumer<string>>>();

            mockConsumer.Setup(c => c.Consume(It.IsAny<TimeSpan>()))
                .Returns(new ConsumeResult<string, string>
                {
                    Message = new Message<string, string> { Value = "test-message" }
                });

            var consumer = new KafkaMessageConsumer<string>(mockLogger.Object, mockConsumer.Object, "test-topic");

            // Act
            var result = consumer.Consume(TimeSpan.FromSeconds(1));

            // Assert
            Assert.Equal("test-message", result);
        }
    }
}
