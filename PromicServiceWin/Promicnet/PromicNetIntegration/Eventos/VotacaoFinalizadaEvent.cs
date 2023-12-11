
using static PromicnetIntegration.TypesDTO;

namespace PromicnetIntegration.Events
{
    /// <summary>
    /// Evento disparado quando uma votação for finalizada
    /// </summary>
    public partial class VotacaoFinalizadaEvent :EventBase
    {
        /// <summary>
        /// Resultado desta votação.
        /// </summary>
        public bool Rsultado { get; }
        /// <summary>
        /// ID da Votação no Promic.
        /// </summary>
        public int Id_votacao { get; }
        /// <summary>
        /// Título da votação.
        /// </summary>
        public string Titulo { get; }
        /// <summary>
        /// Tipos possíveis de votação.
        /// </summary>
        public string[] Tipo_votacao { get; }
        /// <summary>
        /// Lista com todos os votos de uma votação.
        /// </summary>
        public System.Collections.Generic.List<VotoDTO> Votos { get; }
        /// <summary>
        /// Construtor padrão deste evento.
        /// </summary>
        /// <param name="acao">Ação que disparou este evento.</param>
        /// <param name="resultado">Resultado da votação.</param>
        /// <param name="id_votacao">ID da votação no Promic.</param>
        /// <param name="titulo">Título da votação.</param>
        /// <param name="id_promicnet">ID da Reunião no Promicnet.</param>
        /// <param name="tipo_votacao">Tipos possíveis de votação.</param>
        /// <param name="votos">Lista com todos os votos de uma votação.</param>
        /// <param name="id_sala">ID da sala da reunião.</param>
        public VotacaoFinalizadaEvent(string acao, string resultado, int id_votacao, string titulo, string id_promicnet, string[] tipo_votacao, System.Collections.Generic.List<VotoDTO> votos, int id_sala)
        {
            Acao = acao;
            Rsultado = resultado.Equals("APPROVED");
            Id_votacao = id_votacao;
            Titulo = titulo;
            Id_promicnet = id_promicnet;
            Tipo_votacao = tipo_votacao;
            Votos = votos;
            Id_sala = id_sala;
        }
    }
}
