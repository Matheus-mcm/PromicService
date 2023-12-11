namespace PromicnetWebsocket.Messages
{
    /// <summary>
    /// Classe responsável pela mensagem de pausa de votação.
    /// </summary>
    public class PausaVotacao : Message
    {
        /// <summary>
        /// Propriedade que dita se a votação está pausada ou não. Utilize True para pausar a votação e False para despausá-la.
        /// </summary>
        public bool pause { get; set; }
    }
}
