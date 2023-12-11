namespace PromicnetIntegration.Types
{
    /// <summary>
    /// Classe responsável por gerenciar os clientes usados na integração com a API da PromicNet.
    /// Orgao = Casa legislativa
    /// </summary>
    public class Orgao
    {
        /// <summary>
        /// Chave primária do Orgao
        /// </summary>
        public string ID_ORGAO { get; set; }
        /// <summary>
        /// Nome do Orgao
        /// </summary>
        public string NOME { get; set; }
        /// <summary>
        /// Lista de todos os usuários do cliente
        /// </summary>
        public System.Collections.Generic.List<string> DELEGADOS { get; set; }
        /// <summary>
        /// Número de participantes permitidos à ingressas remotamente em uma reunião
        /// </summary>
        public int MAX_PARTICIPANTES { get; set; }
    }
}

