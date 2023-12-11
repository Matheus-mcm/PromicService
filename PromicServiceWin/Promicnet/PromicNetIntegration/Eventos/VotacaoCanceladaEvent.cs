namespace PromicnetIntegration.Events
{
    /// <summary>
    /// Evento disparado quando uma votação for cancelada.
    /// </summary>
    public partial class VotacaoCanceladaEvent :EventBase
    {
        /// <summary>
        /// ID da votação no Promic.
        /// </summary>
        public int Id_votacao { get; }
        /// <summary>
        /// Construtor padrão deste evento.
        /// </summary>
        /// <param name="acao">Ação que disparou deste evento.</param>
        /// <param name="id_promicnet">ID da Reunião no Promicnet.</param>
        /// <param name="id_votacao">ID da votação no Promic.</param>
        /// <param name="id_sala">ID da sala da votação.</param>
        public VotacaoCanceladaEvent(string acao, string id_promicnet, int id_votacao, int id_sala)
        {
            Acao = acao;
            Id_promicnet = id_promicnet;
            Id_votacao = id_votacao;
            Id_sala = id_sala;
        }
    }
}
