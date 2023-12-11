namespace PromicnetIntegration.Types
{
    /// <summary>
    /// Classe responsável por gerenciar as sessões plenárias na interação com a API da PromicNet.
    /// </summary>
    public class Reuniao
    {
        /// <summary>
        /// Status da sessão
        /// </summary>
        public string SITUACAO { get; set; }
        /// <summary>
        /// Nome da sessão
        /// </summary>
        public string DESCRICAO { get; set; }
        /// <summary>
        /// Data de início da sesão
        /// </summary>
        public string DT_INICIO { get; set; }
        /// <summary>
        /// Parte atual da sessão
        /// </summary>
        public string ID_PRMOICNET { get; set; }
        /// <summary>
        /// ID do órgão
        /// </summary>
        public string ID_ORGAO { get; set; }
    }
}

