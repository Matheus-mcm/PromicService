namespace PromicnetIntegration.Types
{
    /// <summary>
    /// Classe responsável por gerenciar os usuários usados na integração com a API da PromicNet.
    /// Usuário = Parlamentar
    /// </summary>
    public class Delegado
    {
        /// <summary>
        /// User_email do usuário
        /// </summary>
        public string LOGIN { get; set; }
        /// <summary>
        /// ID do Orgao
        /// </summary>
        public string ID_ORGAO { get; set; }
        /// <summary>
        /// Nome completo do usuário
        /// </summary>
        public string NOME { get; set; }
        /// <summary>
        /// ID do Delegado
        /// </summary>
        public int ID_DELEGADO { get; set; }
    }
}

