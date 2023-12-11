namespace PromicnetIntegration.Events
{
    /// <summary>
    /// Classe base para os eventos recebidos do WebSocket
    /// </summary>
    public class EventBase
    {
        /// <summary>
        /// Ação que dispara o evento.
        /// </summary>
        public string Acao { get;  set; }
        /// <summary>
        /// ID da reunião no promicnet.
        /// </summary>
        public string Id_promicnet { get;  set; }
        /// <summary>
        /// ID da sala atual.
        /// </summary>
        public int Id_sala { get;  set; }
    }
}
