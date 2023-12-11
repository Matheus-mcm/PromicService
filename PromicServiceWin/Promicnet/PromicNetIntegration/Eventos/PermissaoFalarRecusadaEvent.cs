namespace PromicnetIntegration.Events
{
    /// <summary>
    /// Evento disparado quando um pedido de palavra for recusado pelo Promic
    /// </summary>
    public partial class PermissaoFalarRecusadaEvent : EventBase
    {
        /// <summary>
        /// User_email (email) do participante
        /// </summary>
        public string Login { get; }
        /// <summary>
        /// Construtor padrão deste evento
        /// </summary>
        /// <param name="action">Ação que disparou este evento</param>
        /// <param name="id_promicnet">ID da Reunião no Promicnet</param>
        /// <param name="login">EMail do participante</param>
        /// <param name="id_sala">ID da sala da reunião.</param>
        public PermissaoFalarRecusadaEvent(string action, string id_promicnet, string login, int id_sala)
        {
            this.Acao = action;
            this.Id_promicnet = id_promicnet;
            this.Login = login;
            this.Id_sala = id_sala;
        }
    }
}
