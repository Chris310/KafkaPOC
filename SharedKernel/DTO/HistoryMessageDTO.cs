namespace Infrastructure.Shared.Messaging.DTO
{
    public class HistoryMessageDTO
    {
        /// <summary>
        /// Timestamp of the event.
        /// </summary>
        public DateTime Timestamp { get; set; }

        public string? Data1 { get; set; }
        public string? Data2 { get; set; }
        public string? Data3 { get; set; }
    }
}
