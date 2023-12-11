namespace PromicnetIntegration.Events
{
    /// <summary>
    /// Evento disparado quando um participante vota em uma votação.
    /// </summary>
    public partial class ParticipanteVotouEvent : EventBase
    {
        /// <summary>
        /// ID da Votação do Promic.
        /// </summary>
        public int Id_votacao { get; }
        /// <summary>
        /// Voto do participante.
        /// </summary>
        public string Voto { get; }
        /// <summary>
        /// Email do Participante.
        /// </summary>
        public string Participante { get; }
        /// <summary>
        /// 
        /// </summary>
        public int Id_delegado { get; }
        /// <summary>
        /// Contrutor padrão deste evento.
        /// </summary>
        /// <param name="acao">Ação que disparou o evento.</param>
        /// <param name="id_votacao">ID da votação do Promic.</param>
        /// <param name="id_promicnet">ID da Reunião no Promicnet.</param>
        /// <param name="voto">Voto do participante.</param>
        /// <param name="participante">Email do participante.</param>
        /// <param name="id_delegado">ID do participante no Promic.</param>
        /// <param name="id_sala">ID da sala da reunião.</param>
        public ParticipanteVotouEvent(string acao, int id_votacao, string id_promicnet, string voto, string participante, int id_delegado, int id_sala)
        {
            Acao = acao;
            Id_votacao = id_votacao;
            Id_promicnet = id_promicnet;
            Voto = voto;
            Participante = participante;
            Id_delegado = id_delegado;
            Id_sala = id_sala;
        }
    }
}
