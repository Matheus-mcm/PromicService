namespace PromicnetIntegration.Events
{
    /// <summary>
    /// Evento disparado quando for desativado o vídeo de um participante
    /// </summary>
    public partial class VideoDesativadoEvent : EventBase
    {
        /// <summary>
        /// ID do Agora deste participante
        /// </summary>
        public uint Id_agora { get; }
        /// <summary>
        /// User_email (User_email) deste participante
        /// </summary>
        public string Login { get; }
        /// <summary>
        /// Construtor padrão deste evento
        /// </summary>
        /// <param name="action">Ação que disparou este evento</param>
        /// <param name="login">User_email do participante</param>
        /// <param name="id_sala">ID da sala da reunião.</param>
        public VideoDesativadoEvent(string action, string login, int id_sala)
        {
            this.Acao = action;
            this.Login = login;
            this.Id_sala = id_sala;
        }
    }
}
