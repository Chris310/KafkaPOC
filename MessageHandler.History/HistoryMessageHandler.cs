using Confluent.Kafka;
using Infrastructure.Shared.Messaging.DTO;
using Microsoft.Data.SqlClient;
using SharedKernel.Messaging;
using System.Data;

namespace MessageHandler.History
{
    public class HistoryMessageHandler : IBatchMessageHandler<HistoryMessageDTO>
    {
        private readonly ILogger<HistoryMessageHandler> _logger;

        public HistoryMessageHandler(ILogger<HistoryMessageHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleBatchAsync(IEnumerable<HistoryMessageDTO> messages)
        {
            _logger.LogInformation("Start processing {Count} messages in batch.", messages.Count());

            await InsertHistoricoBatchAsync(messages);

            _logger.LogInformation("Finished processing batch of {Count} messages.", messages.Count());
        }

        public async Task InsertHistoricoBatchAsync(IEnumerable<HistoryMessageDTO> batch)
        {
            var connectionString = "PONERRRRRRconnectionString";

            try
            {
                using var conn = new SqlConnection(connectionString);
                await conn.OpenAsync();

                // Hacemos un bucle para cada mensaje.
                // (Para un "batch real" en un solo SP, usar Table-Valued Parameters, etc.)
                foreach (var msg in batch)
                {
                    try
                    {
                        Console.WriteLine($"Proceso mensaje del topic History. {msg.Data1}, {msg.Timestamp.ToString()}");

                        using var cmd = new SqlCommand("NOMBREsp_DeInsertHistorialSession", conn)
                        {
                            CommandType = CommandType.StoredProcedure
                        };

                        cmd.Parameters.Add("@Param1", SqlDbType.NVarChar, 100).Value = msg.Data1 ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Param2", SqlDbType.NVarChar, 100).Value = msg.Data2 ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Param3", SqlDbType.NVarChar, 100).Value = msg.Data3 ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Timestamp", SqlDbType.DateTime2).Value = msg.Timestamp;

                        await cmd.ExecuteNonQueryAsync();

                        _logger.LogInformation(
                            "Inserted message: Data1={Data1}, Data2={Data2}, Data3={Data3}, Timestamp={Timestamp}",
                            msg.Data1, msg.Data2, msg.Data3, msg.Timestamp
                        );
                    }
                    catch (Exception exMsg)
                    {
                        _logger.LogError(
                            exMsg,
                            "Error inserting message: Data1={Data1}, Data2={Data2}, Data3={Data3}, Timestamp={Timestamp}",
                            msg.Data1, msg.Data2, msg.Data3, msg.Timestamp
                        );

                        throw;
                    }
                }
            }
            catch (Exception exConn)
            {
                _logger.LogError(
                    exConn,
                    "Error opening SQL connection or executing stored procedure in InsertHistoricoBatchAsync."
                );
                throw;
            }
        }
    }
}
