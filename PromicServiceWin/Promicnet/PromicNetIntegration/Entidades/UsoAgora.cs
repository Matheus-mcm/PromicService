namespace PromicnetIntegration.Types
{
    /// <summary>
    /// Classe responsável por gerenciar as informações de uso do Agora
    /// </summary>
    public class UsoAgora
    {
        /// <summary>
        /// Email do participante
        /// </summary>
        public string LOGIN { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CONNECTED_TO { get; set; }
        /// <summary>
        /// Data do início do consumo
        /// </summary>
        public System.DateTime INICIO { get; set; }
        /// <summary>
        /// Data do final do consumo
        /// </summary>
        public System.DateTime FINAL { get; set; }
        /// <summary>
        /// segundos
        /// </summary>
        public string SEGUNDOS { get; set; }
        /// <summary>
        /// tipo
        /// </summary>
        public string TIPO { get; set; }
        /// <summary>
        /// id
        /// </summary>
        public string ID { get; set; }
    }
}

