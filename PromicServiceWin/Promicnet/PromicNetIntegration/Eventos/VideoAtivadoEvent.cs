namespace PromicnetIntegration.Events
{
    /// <summary>
    /// Evento disparado quando for ativado o vídeo de um participante
    /// </summary>
    public partial class VideoAtivadoEvent : EventBase 
    {
        /// <summary>
        /// User_email (User_email) deste participante
        /// </summary>
        public string Login { get; }
        /// <summary>
        /// Construtor padrão deste evento.
        /// </summary>
        /// <param name="action">Ação que disparou o evento.</param>
        /// <param name="login">Email do participante.</param>
        /// <param name="id_sala">ID da sala da reunião.</param>
        public VideoAtivadoEvent(string action, string login, int id_sala)
        {
            this.Acao = action;
            this.Login = login;
            this.Id_sala = id_sala;
        }
    }
}
