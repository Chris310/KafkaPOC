using Infrastructure.Messaging.Kafka;
using SharedKernel.Configuration;
using NLog.Extensions.Logging;
using Infrastructure.Shared.Messaging;
using Infrastructure.Messaging.Kafka.Serialization;
using Infrastructure.Shared.Messaging.DTO;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Configuración de Kafka desde appsettings.json
        services.Configure<MessagingConfiguration>(context.Configuration.GetSection("kafka"));

        // Registrar el serializador de mensajes (JSON en este caso)
        services.AddSingleton<IMessageSerializer, JsonMessageSerializer>();

        // Registrar la fábrica de Kafka
        services.AddSingleton<IMessageBusFactory, KafkaFactory>();

        // Registrar un productor para el tópico "History"
        services.AddSingleton(sp =>
        {
            var factory = sp.GetRequiredService<IMessageBusFactory>();
            var logger = sp.GetRequiredService<ILogger<IMessageProducer<HistoryMessageDTOv2>>>();
            return factory.CreateProducer<HistoryMessageDTOv2>("History", logger);
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
