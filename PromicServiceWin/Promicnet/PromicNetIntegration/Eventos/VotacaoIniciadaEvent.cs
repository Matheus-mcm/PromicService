namespace PromicnetIntegration.Events
{
    /// <summary>
    /// Evento disparado quando uma votação for iniciada.
    /// </summary>
    public partial class VotacaoIniciadaEvent : EventBase
    {
        /// <summary>
        /// ID da Votação no Promic.
        /// </summary>
        public int Id_votacao { get; }
        /// <summary>
        /// Título da votação.
        /// </summary>
        public string Titulo { get; }
        /// <summary>
        /// Ementa da votação.
        /// </summary>
        public string Ementa { get; }
        /// <summary>
        /// Tipos de votação possíveis.
        /// </summary>
        public string[] Tipo_votacao { get; }
        /// <summary>
        /// Situação da votação.
        /// </summary>
        public string Situacao { get; }
        /// <summary>
        /// Tempo de votação.
        /// </summary>
        public int Tempo_votacao { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="acao"></param>
        /// <param name="id_votacao"></param>
        /// <param name="titulo"></param>
        /// <param name="ementa"></param>
        /// <param name="status"></param>
        /// <param name="tipo_votacao"></param>
        /// <param name="tempo_votacao"></param>
        /// <param name="id_sala"></param>
        public VotacaoIniciadaEvent(string acao, int id_votacao, string titulo, string ementa, string status, string[] tipo_votacao, int tempo_votacao, int id_sala)
        {
            Acao = acao;
            Id_votacao = id_votacao;
            Titulo = titulo;
            Ementa = ementa;
            Tipo_votacao = tipo_votacao;
            Tempo_votacao = tempo_votacao;
            Situacao = status;
            Id_sala = id_sala;
        }
    }
}
