
using static PromicnetIntegration.TypesDTO;

namespace PromicnetIntegration.Events
{
    /// <summary>
    /// Classe de evento genérico
    /// </summary>
    public partial class Event
    {
        /// <summary>
        /// Descrição da ação que está disparando este evento
        /// </summary>
        public string Action { get; set; } = string.Empty;
        /// <summary>
        /// ID da Conexão 
        /// </summary>
        public string Connection_id { get; set; } = string.Empty;
        /// <summary>
        /// ID da Reunião que disparando o evento
        /// </summary>
        public string Session_id { get; set; } = string.Empty;
        /// <summary>
        /// User_email do participante
        /// </summary>
        public string User_email { get; set; } = string.Empty;
        /// <summary>
        /// ID do Agora do participante
        /// </summary>
        public string Agora_id { get; set; } = string.Empty;
        /// <summary>
        /// Estado do microfone
        /// </summary>
        public string State { get; set; } = string.Empty;
        /// <summary>
        /// Status da sessão
        /// </summary>
        public string Status { get; set; } = string.Empty;
        /// <summary>
        /// Tipo de mídia, sendo eles Audio ou vídeo
        /// </summary>
        public string Media_type { get; set; } = string.Empty;
        /// <summary>
        /// ID da votação no Promic
        /// </summary>
        public int Vote_id { get; set; } = 0;
        /// <summary>
        /// Título da votação
        /// </summary>
        public string Title { get; set; } = string.Empty;
        /// <summary>
        /// Ementa da votação
        /// </summary>
        public string Description { get; set; } = string.Empty;
        /// <summary>
        /// Tipos de votação possíveis
        /// </summary>
        public string[] Types_allowed { get; set; }
        /// <summary>
        /// Tempo de votação
        /// </summary>
        public int Vote_time { get; set; } = 0;
        /// <summary>
        /// Voto
        /// </summary>
        public string Vote { get; set; } = string.Empty;
        /// <summary>
        /// Resultado da votação
        /// </summary>
        public string Result { get; set; } = string.Empty;
        /// <summary>
        /// Lista com todos os votos
        /// </summary>
        public System.Collections.Generic.List<VotoDTO> Votes { get; set; } = new System.Collections.Generic.List<VotoDTO>();
        /// <summary>
        /// 
        /// </summary>
        public int User_id { get; set; } = 0;
    }
}
