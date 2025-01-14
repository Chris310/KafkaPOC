
namespace SharedKernel.Configuration
{
    public class MessagingConfiguration
    {
        public string BootstrapServers { get; set; }

        public Dictionary<string, TopicConfiguration> Topics { get; set; } = new();

        public ProducerOptions ProducerOptions { get; set; } = new();

        public ConsumerOptions ConsumerOptions { get; set; } = new();
    }

    public class ProducerOptions
    {
        public string Acks { get; set; } // "All", "None", "Leader"
        public int? LingerMs { get; set; }
        public int? BatchNumMessages { get; set; }
        public int? MessageTimeoutMs { get; set; }
        // Aquí se puede agregar lo que Confluent te permita configurar
        // (SecurityProtocol, SslCaLocation, etc.)
    }

    public class ConsumerOptions
    {
        public string? GroupId { get; set; }
        public bool? EnableAutoCommit { get; set; }
        public int? FetchWaitMaxMs { get; set; }
        public string? AutoOffsetReset { get; set; }  // "Earliest", "Latest", "None"
    }

    // Por cada topic, mantienes tus campos
    public class TopicConfiguration
    {
        public string Name { get; set; }
        public int Partitions { get; set; }
        public short ReplicationFactor { get; set; }
        public int? MaxPollRecords { get; set; }
        public int? FetchWaitMaxMs { get; set; }
        public bool? EnableAutoCommit { get; set; }
    }

}
