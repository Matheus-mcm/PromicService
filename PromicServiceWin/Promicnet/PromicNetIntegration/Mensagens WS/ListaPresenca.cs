using System.Collections.Generic;

namespace PromicnetWebsocket.Messages
{
    /// <summary>
    /// 
    /// </summary>
    public class ListaPresenca : Message
    {
        /// <summary>
        /// Lista com todos os participantes da reunião
        /// </summary>
        public List<Presence> participants { get; set; }
        /// <summary>
        /// Construtor padrão da classe de lista de presença
        /// </summary>
        /// <param name="session_sk">ID da sessão no Promicnet.</param>
        /// <param name="participants">Lista de participantes da reunião</param>
        public ListaPresenca(string session_sk, List<Presence> participants)
        {
            action = "list_participants";
            session_id = session_sk;
            this.participants = participants;
        }
    }
}
