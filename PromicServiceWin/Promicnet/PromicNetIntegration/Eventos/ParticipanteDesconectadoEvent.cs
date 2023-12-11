namespace PromicnetIntegration.Events
{
    /// <summary>
    /// Evento disparado quando um participante sair da reunião
    /// </summary>
    public partial class ParticipanteDesconectadoEvent : EventBase
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
        /// <param name="action">Ação que disparou o evento</param>
        /// <param name="id_agora">ID do Agora</param>
        /// <param name="login">User_email do participante</param>
        /// <param name="id_sala">ID da sala da reunião.</param>
        public ParticipanteDesconectadoEvent(string action, string id_agora, string login, int id_sala)
        {
            this.Acao = action;
            this.Login = login;
            this.Id_agora = System.Convert.ToUInt32(id_agora);
            this.Id_sala = id_sala;
        }
    }
}
