using Confluent.Kafka;
using Infrastructure.Shared.Messaging.DTO;
using Microsoft.Data.SqlClient;
using Infrastructure.Shared.Messaging;
using System.Data;

namespace MessageHandler.History
{
    public class HistoryMessageHandler : IBatchMessageHandler<HistoryMessageDTOv2>
    {
        private readonly ILogger<HistoryMessageHandler> _logger;

        public HistoryMessageHandler(ILogger<HistoryMessageHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleBatchAsync(IEnumerable<HistoryMessageDTOv2> messages)
        {
            _logger.LogInformation($"Mando a procesar {messages.Count().ToString()} mensajes.");
            Console.WriteLine($"Mando a procesar {messages.Count().ToString()} mensajes.");

            await InsertHistoricoBatchAsync(messages);

            _logger.LogInformation("Terminó de procesar el batch.");
        }

        public async Task InsertHistoricoBatchAsync(IEnumerable<HistoryMessageDTOv2> batch)
        {
            //var connectionString = "PONERRRRRRconnectionString";

            try
            {
                //using var conn = new SqlConnection(connectionString);
                //await conn.OpenAsync();

                // Hacemos un bucle para cada mensaje.
                // (Para un "batch real" en un solo SP, usar Table-Valued Parameters, etc.)
                foreach (var message in batch)
                {
                    try
                    {
                        Console.WriteLine($"Proceso mensaje {message.Fecha.ToString()}, {message.InfoSolicitud ?? "N/A"}");

                        //using var cmd = new SqlCommand("NOMBREsp_DeInsertHistorialSession", conn)
                        //{
                        //    CommandType = CommandType.StoredProcedure
                        //};

                        //cmd.Parameters.Add("@Param1", SqlDbType.NVarChar, 100).Value = msg.Data1 ?? (object)DBNull.Value;
                        //cmd.Parameters.Add("@Param2", SqlDbType.NVarChar, 100).Value = msg.Data2 ?? (object)DBNull.Value;
                        //cmd.Parameters.Add("@Param3", SqlDbType.NVarChar, 100).Value = msg.Data3 ?? (object)DBNull.Value;
                        //cmd.Parameters.Add("@Timestamp", SqlDbType.DateTime2).Value = msg.Timestamp;

                        //await cmd.ExecuteNonQueryAsync();

                        _logger.LogInformation("Inserted message: InfoPublica={InfoPublica}, InfoPrivada={InfoPrivada}, Fecha={Fecha}",
                            message.InfoPublica, message.InfoPrivada, message.Fecha
                        );
                    }
                    catch (Exception exMsg)
                    {
                        _logger.LogError(
                            exMsg,
                            "Error inserting message: InfoPublica={InfoPublica}, InfoPrivada={InfoPrivada}, Fecha={Fecha}",
                            message.InfoPublica, message.InfoPrivada, message.Fecha
                        );

                        throw;
                    }
                }
            }
            catch (Exception exConn)
            {
                _logger.LogError(exConn, "Error opening SQL connection or executing stored procedure in InsertHistoricoBatchAsync.");
                throw;
            }
        }
    }
}
