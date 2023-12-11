namespace PromicnetWebsocket.Messages
{
    /// <summary>
    /// Classe genérica para todas as mensagens.
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Ação que será disparada.
        /// </summary>
        public string action { get; set; }
        /// <summary>
        /// ID da sessão no Promicnet.
        /// </summary>
        public string session_id { get; set; }
        /// <summary>
        /// Construtor vazio da classe.
        /// </summary>
        public Message() { }
        /// <summary>
        /// Construtor utilizado para enviar mensagens de uma sessão.
        /// </summary>
        /// <param name="session_sk">ID da sessão no Promicnet.</param>
        /// <param name="action">Ação que será executada.</param>
        public Message(string action, string session_sk)
        {
            this.action = action;
            session_id = session_sk;
        }
        /// <summary>
        /// Construtor utilizado para enviar mensagens que não necessitam de outras informações.
        /// </summary>
        /// <param name="action">Ação que será executada.</param>
        public Message(string action)
        {
            this.action = action;
        }

    }
}