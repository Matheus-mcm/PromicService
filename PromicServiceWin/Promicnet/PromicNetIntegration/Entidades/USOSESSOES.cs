namespace PromicnetIntegration.Types
{
    /// <summary>
    /// Classe responsável por gerenciar as inforamções das sessões que tem algum uso do Agora
    /// </summary>
    public class USOSESSOES
    {
        /// <summary>
        /// ID da Reunião no Promicnet.
        /// </summary>
        public string ID_PROMICNET { get; set; }
        /// <summary>
        /// Título da Reunião.    
        /// </summary>
        public string DESCRICAO { get; set; }
        /// <summary>
        /// ID do Orgão no Promicnet.
        /// </summary>
        public string ID_ORGAO { get; set; }
    }
}

