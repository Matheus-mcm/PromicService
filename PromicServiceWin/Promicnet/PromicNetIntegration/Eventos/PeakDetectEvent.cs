namespace PromicnetIntegration.Events
{
    /// <summary>
    /// Evento disparado ao realizar um PeakDestect 
    /// </summary>
    public partial class PeakDetectEvent :EventBase
    {
        /// <summary>
        /// User_email (email) do participante
        /// </summary>
        public string Login { get; }
        /// <summary>
        /// Situação do microfone
        /// </summary>
        public bool Situacao { get; }
        /// <summary>
        /// Construtor padrão deste evento
        /// </summary>
        /// <param name="action">Ação que disparou esse evento</param>
        /// <param name="session_id">ID da Reunião no Promicnet</param>
        /// <param name="login">Emaild o participante</param>
        /// <param name="state">situação do microfone</param>
        /// <param name="id_sala">ID da sala da reunião.</param>
        public PeakDetectEvent(string action, string session_id, string login, string state, int id_sala)
        {
            this.Acao = action;
            this.Id_promicnet = session_id;
            this.Login = login;
            this.Situacao = state == "ON";
            this.Id_sala -= id_sala;
        }
    }
}
