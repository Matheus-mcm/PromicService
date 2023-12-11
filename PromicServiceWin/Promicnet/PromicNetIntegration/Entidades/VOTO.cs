namespace PromicnetIntegration.Types
{
    /// <summary>
    /// Classe responsável por gerenciar os votos de uma votação.
    /// </summary>
    public class VOTO
    {
        /// <summary>
        /// Nome do participante votante.
        /// </summary>
        public string NOME_PARTICIPANTE { get; set; }
        /// <summary>
        /// Partido do participante votante.
        /// </summary>
        public string PARTIDO { get; set; }
        /// <summary>
        /// Voto deste participante, condizente com o tipo de votação.
        /// </summary>
        public string VOTO_PARTICIPANTE { get; set; }
        /// <summary>
        /// Indicador se o participante já registrou seu voto ou não
        /// </summary>
        public bool VOTOU { get; set; }
    }
}

