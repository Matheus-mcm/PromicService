namespace PromicnetIntegration.Events
{
    /// <summary>
    /// Evento disparado quando um participante ingressar na reunião
    /// </summary>
    public partial class ParticipanteIngressouEvent : EventBase
    {
        /// <summary>
        /// User_email (email) do participante
        /// </summary>
        public string Login { get; }
        /// <summary>
        ///  ID do Agora do Participante
        /// </summary>
        public uint Id_agora { get; }
        /// <summary>
        /// Construtor padrão deste evento
        /// </summary>
        /// <param name="action">Ação que disparou este evento</param>
        /// <param name="session_id">ID da Reunião no Promicnet</param>
        /// <param name="user_email">Email do participante</param>
        /// <param name="id_agora">ID do Agora</param>
        /// <param name="id_sala">ID da sala da reunião.</param>
        public ParticipanteIngressouEvent(string action, string session_id, string user_email, string id_agora, int id_sala)
        {
            this.Acao = action;
            this.Id_promicnet = session_id;
            this.Login = user_email;
            this.Id_agora = System.Convert.ToUInt32(id_agora);
            this.Id_sala = id_sala;
        }
    }
}
