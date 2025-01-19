using Infrastructure.Messaging.Kafka;
using Infrastructure.Shared.Messaging.Configuration;
using Infrastructure.Shared.Messaging;
using NLog.Extensions.Logging;
using Microsoft.Extensions.Options;
using MicroserviceConsumerA;
using Infrastructure.Messaging.Kafka.Serialization;
using Infrastructure.Shared.Messaging.DTO;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Configuración de Kafka desde appsettings.json
        services.Configure<MessagingConfiguration>(context.Configuration.GetSection("Kafka"));

        // Registrar el serializador de mensajes (JSON en este caso)
        services.AddSingleton<IMessageSerializer, JsonMessageSerializer>();

        // Registrar la fábrica de Kafka
        services.AddSingleton<IMessageBusFactory, KafkaFactory>();

        // Registrar el handler de mensajes
        services.AddSingleton<IMessageHandler<HistoryMessageDTO>, HistoryMessageHandler>();

        // Registrar un consumidor genérico para el tópico "History"
        services.AddSingleton<IMessageConsumer<HistoryMessageDTO>>(sp =>
        {
            var factory = sp.GetRequiredService<IMessageBusFactory>();
            var config = sp.GetRequiredService<IOptions<MessagingConfiguration>>().Value;
            var logger = sp.GetRequiredService<ILogger<KafkaMessageConsumer<HistoryMessageDTO>>>();
            return factory.CreateConsumer<HistoryMessageDTO>("History", config.ConsumerOptions.GroupId, logger);
        });

        services.AddHostedService<Worker>();
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddNLog();
    })
    .Build();

await host.RunAsync();
