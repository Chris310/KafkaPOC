﻿using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Infrastructure.Shared.Messaging.Configuration;
using Infrastructure.Shared.Messaging;

namespace Infrastructure.Messaging.Kafka
{
    public class KafkaFactory : IMessageBusFactory
    {
        private readonly MessagingConfiguration _config;
        private readonly ISchemaRegistryClient _schemaRegistryClient;

        public KafkaFactory(IOptions<MessagingConfiguration> config)
        {
            _config = config?.Value ?? throw new ArgumentNullException(nameof(config));

            var schemaRegistryConfig = new SchemaRegistryConfig
            {
                Url = _config.SchemaRegistry.Url,
                BasicAuthCredentialsSource = AuthCredentialsSource.UserInfo,
                BasicAuthUserInfo = $"{_config.SchemaRegistry.ApiKey}:{_config.SchemaRegistry.ApiSecret}"
            };

            _schemaRegistryClient = new CachedSchemaRegistryClient(schemaRegistryConfig);
        }

        public IMessageProducer<T> CreateProducer<T>(string topicName, ILogger<IMessageProducer<T>> logger) where T : class
        {
            if (!_config.Topics.ContainsKey(topicName))
            {
                logger.LogError("Topic '{TopicName}' is not configured.", topicName);
                throw new ArgumentException($"Topic {topicName} is not configured.");
            }

            // Aseguramos que el topic exista (si no existe, se crea)
            //try
            //{
            //    EnsureTopicExistsAsync(topicName).GetAwaiter().GetResult();
            //}
            //catch (Exception ex)
            //{
            //    logger.LogError(ex, "Failed to ensure topic '{TopicName}' exists.", topicName);
            //    throw;
            //}

            // Configuración base del Producer
            var topicConfig = _config.Topics[topicName];

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = _config.BootstrapServers
            };

            if (!string.IsNullOrEmpty(_config.SaslUsername))
            {
                producerConfig.SaslUsername = _config.SaslUsername;
                producerConfig.SaslPassword = _config.SaslPassword;
                producerConfig.SecurityProtocol = Enum.Parse<SecurityProtocol>(_config.SecurityProtocol, true);
                producerConfig.SaslMechanism = Enum.Parse<SaslMechanism>(_config.SaslMechanism, true);
            }

            var jsonSerializerConfig = new JsonSerializerConfig
            {
                AutoRegisterSchemas = true,
                SubjectNameStrategy = SubjectNameStrategy.Topic
            };

            // ==============================
            // APLICAR ProducerOptions GLOBALES (si existen)
            // ==============================
            var producerOptions = _config.ProducerOptions;

            // Acks (puede ser "All", "None", "Leader")
            if (!string.IsNullOrEmpty(producerOptions?.Acks))
            {
                producerConfig.Acks = Enum.Parse<Acks>(producerOptions.Acks, ignoreCase: true);
            }

            // LingerMs
            if (producerOptions?.LingerMs.HasValue ?? false)
            {
                producerConfig.LingerMs = producerOptions.LingerMs.Value;
            }

            // BatchNumMessages
            if (producerOptions?.BatchNumMessages.HasValue ?? false)
            {
                producerConfig.BatchNumMessages = producerOptions.BatchNumMessages.Value;
            }

            // MessageTimeoutMs
            if (producerOptions?.MessageTimeoutMs.HasValue ?? false)
            {
                producerConfig.MessageTimeoutMs = producerOptions.MessageTimeoutMs.Value;
            }

            // ==============================
            // FIN APLICACIÓN ProducerOptions
            // ==============================

            var topic = _config.Topics[topicName].Name;

            logger.LogInformation("Creating producer for topic '{TopicName}' -> actual topic '{Actual}'.", topicName, topic);


            var producerBuilder = new ProducerBuilder<string, T>(producerConfig)
                .SetValueSerializer(new JsonSerializer<T>(_schemaRegistryClient, jsonSerializerConfig))
                .SetKeySerializer(Serializers.Utf8);

            try
            {
                var producer = producerBuilder.Build();

                return new KafkaMessageProducer<T>(logger, producer, topicConfig.Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create producer for topic '{TopicName}'.", topicName);
                throw;
            }
        }

        public IMessageConsumer<T> CreateConsumer<T>(string topicName, string groupId, ILogger<IMessageConsumer<T>> logger) where T : class
        {
            if (!_config.Topics.ContainsKey(topicName))
            {
                logger.LogError("Topic '{TopicName}' is not configured.", topicName);
                throw new ArgumentException($"Topic {topicName} is not configured.");
            }

            var topicConfig = _config.Topics[topicName];
            var topic = topicConfig.Name;

            if (string.IsNullOrEmpty(groupId) && !string.IsNullOrEmpty(_config.ConsumerOptions.GroupId))
            {
                groupId = _config.ConsumerOptions.GroupId;
            }

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _config.BootstrapServers,
                GroupId = groupId
            };

            if (!string.IsNullOrEmpty(_config.SaslUsername))
            {
                consumerConfig.SaslUsername = _config.SaslUsername;
                consumerConfig.SaslPassword = _config.SaslPassword;
                consumerConfig.SecurityProtocol = Enum.Parse<SecurityProtocol>(_config.SecurityProtocol, true);
                consumerConfig.SaslMechanism = Enum.Parse<SaslMechanism>(_config.SaslMechanism, true);
            }

            var jsonDeserializer = new JsonDeserializer<T>().AsSyncOverAsync();

            // ==============================
            // APLICAR ConsumerOptions GLOBALES (si existen)
            // ==============================
            var consumerOptions = _config.ConsumerOptions;

            // AutoOffsetReset (Earliest, Latest, None)
            if (!string.IsNullOrEmpty(consumerOptions?.AutoOffsetReset))
            {
                consumerConfig.AutoOffsetReset = Enum.Parse<AutoOffsetReset>(consumerOptions.AutoOffsetReset, ignoreCase: true);
            }

            // EnableAutoCommit (global)
            if (consumerOptions?.EnableAutoCommit.HasValue ?? false)
            {
                consumerConfig.EnableAutoCommit = consumerOptions.EnableAutoCommit.Value;
            }

            // FetchWaitMaxMs (global)
            if (consumerOptions?.FetchWaitMaxMs.HasValue ?? false)
            {
                consumerConfig.FetchWaitMaxMs = consumerOptions.FetchWaitMaxMs.Value;
            }

            if (_config.ConsumerOptions.MaxPollIntervalMs.HasValue)
            {
                consumerConfig.MaxPollIntervalMs = _config.ConsumerOptions.MaxPollIntervalMs.Value;
            }

            if (_config.ConsumerOptions.FetchMinBytes.HasValue)
            {
                consumerConfig.FetchMinBytes = _config.ConsumerOptions.FetchMinBytes.Value;
            }

            // ==============================
            // FIN APLICACIÓN ConsumerOptions
            // ==============================

            // APLICAR configuraciones específicas del tópico
            // (si están definidas, sobreescriben lo global)
            if (topicConfig.MaxPollRecords.HasValue)
            {
                consumerConfig.Set("max.poll.records", topicConfig.MaxPollRecords.Value.ToString());
            }

            if (topicConfig.FetchWaitMaxMs.HasValue)
            {
                consumerConfig.FetchWaitMaxMs = topicConfig.FetchWaitMaxMs.Value;
            }

            if (topicConfig.EnableAutoCommit.HasValue)
            {
                consumerConfig.EnableAutoCommit = topicConfig.EnableAutoCommit.Value;
            }

            logger.LogInformation("Creating consumer for topic '{TopicName}' (actual topic '{ActualTopic}') with Group ID '{GroupId}'.", topicName, topic, groupId);


            var consumerBuilder = new ConsumerBuilder<string, T>(consumerConfig)
                .SetValueDeserializer(jsonDeserializer)
                .SetKeyDeserializer(Deserializers.Utf8);

            var consumer = consumerBuilder.Build();

            try
            {
                return new KafkaMessageConsumer<T>(logger, consumer, topicConfig.Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create consumer for topic '{TopicName}'.", topicName);
                throw;
            }
        }

        //public async Task EnsureTopicExistsAsync(string topicName)
        //{
        //    if (!_config.Topics.TryGetValue(topicName, out var topicConfig))
        //    {
        //        throw new ArgumentException($"Topic {topicName} is not configured.");
        //    }

        //    try
        //    {
        //        using var adminClient = new AdminClientBuilder(new AdminClientConfig
        //        {
        //            BootstrapServers = _config.BootstrapServers
        //        }).Build();

        //        var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
        //        var existingTopics = metadata.Topics.Select(t => t.Topic).ToHashSet();

        //        if (existingTopics.Contains(topicConfig.Name))
        //        {
        //            _logger.LogInformation("Topic '{TopicName}' already exists.", topicConfig.Name);
        //            return;
        //        }

        //        var topicSpecification = new TopicSpecification
        //        {
        //            Name = topicConfig.Name,
        //            NumPartitions = topicConfig.Partitions,
        //            ReplicationFactor = topicConfig.ReplicationFactor
        //        };

        //        _logger.LogInformation("Creating topic '{TopicName}' with {Partitions} partitions and replication factor {ReplicationFactor}.", topicConfig.Name, topicConfig.Partitions, topicConfig.ReplicationFactor);

        //        await adminClient.CreateTopicsAsync(new List<TopicSpecification> { topicSpecification });

        //        _logger.LogInformation("Topic '{TopicName}' created successfully.", topicConfig.Name);
        //    }
        //    catch (CreateTopicsException ex)
        //    {
        //        foreach (var result in ex.Results)
        //        {
        //            if (result.Error.Code == ErrorCode.TopicAlreadyExists)
        //            {
        //                _logger.LogWarning("Topic '{TopicName}' already exists (detected in exception).", result.Topic);
        //            }
        //            else
        //            {
        //                _logger.LogError("Error creating topic '{TopicName}': {Reason}", result.Topic, result.Error.Reason);
        //                throw;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "An error occurred while ensuring topic '{TopicName}' exists.", topicName);
        //        throw;
        //    }
        //}
    }
}
