namespace Infrastructure.Shared.Messaging.Configuration
{
    /// <summary>
    /// Configuración principal de mensajería.
    /// </summary>
    public class MessagingConfiguration
    {
        /// <summary>
        /// Lista de servidores Kafka (bootstrap servers).
        /// Ejemplo: "localhost:9092" o "pkc-xxxxx.confluent.cloud:9092".
        /// </summary>
        public string? BootstrapServers { get; set; }

        /// <summary>
        /// Nombre de usuario para la autenticación SASL.
        /// Solo necesario si el cluster requiere autenticación SASL.
        /// </summary>
        public string? SaslUsername { get; set; }

        /// <summary>
        /// Contraseña para la autenticación SASL.
        /// Solo necesario si el cluster requiere autenticación SASL.
        /// </summary>
        public string? SaslPassword { get; set; }

        /// <summary>
        /// Protocolo de seguridad utilizado.
        /// Valores posibles: "SaslSsl", "Plaintext", etc.
        /// </summary>
        public string? SecurityProtocol { get; set; }

        /// <summary>
        /// Mecanismo de autenticación SASL.
        /// Valores posibles: "Plain", "ScramSha256", "ScramSha512", etc.
        /// </summary>
        public string? SaslMechanism { get; set; }

        /// <summary>
        /// Configuración para el Schema Registry.
        /// </summary>
        public SchemaRegistryConfiguration SchemaRegistry { get; set; } = new();

        /// <summary>
        /// Configuración de los topics disponibles.
        /// Cada topic se configura por su nombre.
        /// </summary>
        public Dictionary<string, TopicConfiguration> Topics { get; set; } = new();

        /// <summary>
        /// Configuración específica para el productor de mensajes.
        /// </summary>
        public ProducerOptions ProducerOptions { get; set; } = new();

        /// <summary>
        /// Configuración específica para el consumidor de mensajes.
        /// </summary>
        public ConsumerOptions ConsumerOptions { get; set; } = new();
    }

    /// <summary>
    /// Configuración para el Schema Registry.
    /// </summary>
    public class SchemaRegistryConfiguration
    {
        /// <summary>
        /// URL del Schema Registry.
        /// Ejemplo: "https://xxxx.confluent.cloud".
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// API Key para autenticar con el Schema Registry.
        /// </summary>
        public string? ApiKey { get; set; }

        /// <summary>
        /// API Secret para autenticar con el Schema Registry.
        /// </summary>
        public string? ApiSecret { get; set; }
    }

    /// <summary>
    /// Opciones de configuración para productores de mensajes.
    /// </summary>
    public class ProducerOptions
    {
        /// <summary>
        /// Nivel de confirmación requerido por el productor.
        /// Valores posibles: "All", "None", "Leader".
        /// Default: "All".
        /// </summary>
        public string? Acks { get; set; }

        /// <summary>
        /// Tiempo (en milisegundos) que el productor espera para agrupar mensajes.
        /// Default: 0 (sin retraso).
        /// </summary>
        public int? LingerMs { get; set; }

        /// <summary>
        /// Número máximo de mensajes que puede acumularse en un batch.
        /// Default: 10000.
        /// </summary>
        public int? BatchNumMessages { get; set; }

        /// <summary>
        /// Tiempo máximo (en milisegundos) para que un mensaje sea entregado.
        /// Default: 30000 ms.
        /// </summary>
        public int? MessageTimeoutMs { get; set; }
    }

    /// <summary>
    /// Opciones de configuración para consumidores de mensajes.
    /// </summary>
    public class ConsumerOptions
    {
        /// <summary>
        /// Grupo de consumidores al que pertenece este consumidor.
        /// Los consumidores en el mismo grupo comparten la carga.
        /// </summary>
        public string? GroupId { get; set; }

        /// <summary>
        /// Indica si los commits se hacen automáticamente.
        /// Default: true.
        /// </summary>
        public bool? EnableAutoCommit { get; set; }

        /// <summary>
        /// Tiempo máximo que el broker espera antes de devolver un conjunto de mensajes.
        /// Default: 500 ms.
        /// </summary>
        public int? FetchWaitMaxMs { get; set; }

        /// <summary>
        /// Configura dónde empezar a leer mensajes si no hay un offset guardado.
        /// Valores posibles: "Earliest", "Latest", "None".
        /// Default: "Latest".
        /// </summary>
        public string? AutoOffsetReset { get; set; }

        /// <summary>
        /// Tiempo máximo (en milisegundos) que un consumidor puede tardar en procesar mensajes antes de ser considerado inactivo.
        /// Default: 300000 ms (5 minutos).
        /// </summary>
        public int? MaxPollIntervalMs { get; set; }

        /// <summary>
        /// Tamaño mínimo de datos (en bytes) que el broker debe acumular antes de responder a una solicitud de consumo.
        /// Default: 1 byte.
        /// </summary>
        public int? FetchMinBytes { get; set; }
    }

    /// <summary>
    /// Configuración específica para un topic de Kafka.
    /// </summary>
    public class TopicConfiguration
    {
        /// <summary>
        /// Nombre del topic.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Número de particiones del topic.
        /// Default: 1.
        /// </summary>
        public int Partitions { get; set; }

        /// <summary>
        /// Factor de replicación del topic.
        /// Default: 1.
        /// </summary>
        public short ReplicationFactor { get; set; }

        /// <summary>
        /// Número máximo de mensajes que se devuelven por poll.
        /// Default: 500.
        /// </summary>
        public int? MaxPollRecords { get; set; }

        /// <summary>
        /// Tiempo máximo que el broker espera antes de devolver un conjunto de mensajes para este topic.
        /// Default: 500 ms.
        /// </summary>
        public int? FetchWaitMaxMs { get; set; }

        /// <summary>
        /// Indica si los commits automáticos están habilitados para este topic.
        /// Default: true.
        /// </summary>
        public bool? EnableAutoCommit { get; set; }
    }
}
