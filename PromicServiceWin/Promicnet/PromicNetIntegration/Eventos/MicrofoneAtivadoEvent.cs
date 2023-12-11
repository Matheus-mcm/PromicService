namespace PromicnetIntegration.Events
{
    /// <summary>
    /// Evento disparado quando o microfone de um participante for ativado 
    /// </summary>
    public partial class MicrofoneAtivadoEvent : EventBase
    {
        /// <summary>
        /// Email do participante
        /// </summary>
        public string Login { get; }
        /// <summary>
        /// Construtor pardão deste evento
        /// </summary>
        /// <param name="action">Ação que disparou esse evento</param>
        /// <param name="user_email">Email do participante</param>
        /// <param name="id_sala">ID da sala da reunião.</param>
        public MicrofoneAtivadoEvent(string action, string user_email, int id_sala)
        {
            this.Acao = action;
            this.Login = user_email;
            this.Id_sala = id_sala;
        }
    }
}
