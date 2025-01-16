namespace SharedKernel.Configuration
{
    public class MessagingConfiguration
    {
        public string? BootstrapServers { get; set; }

        public string? SaslUsername { get; set; }
        public string? SaslPassword { get; set; }
        public string? SecurityProtocol { get; set; } // "SaslSsl", "Plaintext", etc.
        public string? SaslMechanism { get; set; }    // "Plain", "ScramSha256", etc.

        public SchemaRegistryConfiguration SchemaRegistry { get; set; } = new();

        public Dictionary<string, TopicConfiguration> Topics { get; set; } = [];

        public ProducerOptions ProducerOptions { get; set; } = new();
        public ConsumerOptions ConsumerOptions { get; set; } = new();
    }

    public class SchemaRegistryConfiguration
    {
        public string? Url { get; set; }
        public string? ApiKey { get; set; }
        public string? ApiSecret { get; set; }
    }

    public class ProducerOptions
    {
        public string? Acks { get; set; } // "All", "None", "Leader"
        public int? LingerMs { get; set; }
        public int? BatchNumMessages { get; set; }
        public int? MessageTimeoutMs { get; set; }
    }

    public class ConsumerOptions
    {
        public string? GroupId { get; set; }
        public bool? EnableAutoCommit { get; set; }
        public int? FetchWaitMaxMs { get; set; }
        public string? AutoOffsetReset { get; set; }  // "Earliest", "Latest", "None"
    }

    public class TopicConfiguration
    {
        public string? Name { get; set; }
        public int Partitions { get; set; }
        public short ReplicationFactor { get; set; }
        public int? MaxPollRecords { get; set; }
        public int? FetchWaitMaxMs { get; set; }
        public bool? EnableAutoCommit { get; set; }
    }
}
