﻿namespace Infrastructure.Shared.Messaging.DTO
{
    public class HistoryMessageDTOv2
    {
        public DateTime Fecha { get; set; }
        public string? InfoPublica { get; set; }
        public string? InfoPrivada { get; set; }
        public string? InfoSolicitud { get; set; }
        public string? InfoRespuesta { get; set; }
    }
}
