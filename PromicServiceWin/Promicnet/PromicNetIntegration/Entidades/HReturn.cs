namespace PromicnetIntegration.Types
{
    /// <summary>
    /// Classe de retorno de chamas
    /// </summary>
    public class HReturn<T>
    {
        /// <summary>
        /// Status do retorno da chamada
        /// </summary>
        public bool SUCESSO { get; set; }
        /// <summary>
        /// Objeto retornado
        /// </summary>
        public T OBJETO { get; set; }
        /// <summary>
        /// Mensagem de retorno
        /// </summary>
        public string MENSAGEM { get; set; }
    }
}

