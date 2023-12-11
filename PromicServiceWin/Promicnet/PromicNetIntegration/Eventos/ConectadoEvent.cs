namespace PromicnetIntegration.Events
{
    /// <summary>
    /// Evento disparado quando um participante conectar-se à reunião
    /// </summary>
    public partial class ConectadoEvent : EventBase
    {
        /// <summary>
        /// ID da Conexão deste participante
        /// </summary>
        public string Id_conexao { get; }
        /// <summary>
        /// ID do Agora deste participante
        /// </summary>
        public uint Id_agora { get; }
        /// <summary>
        /// Construtor padrão do Evento
        /// </summary>
        /// <param name="action">Ação que está chamando o evento</param>
        /// <param name="connection_id">ID da Conexão</param>
        /// <param name="id_sala">ID da sala da reunião.</param>
        public ConectadoEvent(string action, string connection_id, int id_sala)
        {
            this.Acao = action;
            this.Id_conexao = connection_id;
            this.Id_sala = id_sala;
        }
    }
}
