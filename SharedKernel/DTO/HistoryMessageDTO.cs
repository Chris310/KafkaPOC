namespace Infrastructure.Shared.Messaging.DTO
{
    public class HistoryMessageDTO
    {
        /// <summary>
        /// Timestamp of the event.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Data associated with the historical message.
        /// </summary>
        public string Data { get; set; }
    }
}
