namespace PromicnetIntegration.Types
{
    /// <summary>
    /// Classe de log
    /// </summary>
    public class LogData
    {
        /// <summary>
        /// Data e hora em que a mensagem foi gerada.
        /// </summary>
        public System.DateTime Time { get; private set; }
        /// <summary>
        /// Identificador de qual classe está sendo enviada a mensagem. Atualmente podendo ser: API ou WebSocket.
        /// </summary>
        public string Class { get; private set; }
        /// <summary>
        /// Identificador de qual classe está sendo enviada a mensagem. Atualmente podendo ser: API ou WebSocket.
        /// </summary>
        public string Status { get; private set; }
        /// <summary>
        /// Nome da função que disparou o evento.
        /// </summary>
        public string Function { get; private set; }
        /// <summary>
        /// A mensagem de log a ser exibida.
        /// </summary>
        public string Message { get; private set; }
        /// <summary>Construtor padrão da classe de log.</summary>
        /// <param name="status"></param>
        /// <param name="class">Classe atual.</param>
        /// <param name="function">Função atual.</param>
        /// <param name="message">Mensagem a ser exibida.</param>
        public LogData(string @class, string status, string function, string message)
        {
            Class = @class;
            Status = status;
            Function = function;
            Message = message;
            Time = System.DateTime.Now;
        }
    }
}

