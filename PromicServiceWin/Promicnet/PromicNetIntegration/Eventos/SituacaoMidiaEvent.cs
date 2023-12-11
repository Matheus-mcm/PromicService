namespace PromicnetIntegration.Events
{
    /// <summary>
    /// Evento disparado quando a situação de um dos dispositivos de mídia, microfone ou câmera, tiver a situação aleterada
    /// </summary>
    public partial class SituacaoMidiaEvent : EventBase
    {
        /// <summary>
        /// Loigin (email) do usuário
        /// </summary>
        public string Login { get; }
        /// <summary>
        /// Tipo de mídia
        /// </summary>
        public string Media { get; }
        /// <summary>
        /// Situação da mídia, caso seja true o dispositivo está ligado.
        /// </summary>
        public bool Situacao { get; }
        /// <summary>
        /// Construtor padrão deste evento
        /// </summary>
        /// <param name="action">Ação que disparou este evento</param>
        /// <param name="login">Email do participante</param>
        /// <param name="media">Tipo de mídia que foi alterada, podendo ser: audio ou video.</param>
        /// <param name="situacao">Situação da mídia, podendo ser "ON" ou "OFF".</param>
        /// <param name="session_id">ID da Reunião do Promicnet</param>
        /// <param name="id_sala">ID da sala da reunião.</param>
        public SituacaoMidiaEvent(string action, string login, string media, string situacao, string session_id, int id_sala)
        {
            this.Login = login; 
            this.Media = media;
            this.Id_promicnet = session_id;
            this.Situacao = situacao == "ON";
            this.Acao = action;
            this.Id_sala += id_sala;
        }
    }
}
