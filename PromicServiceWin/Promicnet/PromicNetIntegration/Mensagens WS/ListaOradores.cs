using System.Collections.Generic;

namespace PromicnetWebsocket.Messages
{
    /// <summary>
    /// 
    /// </summary>
    public class ListaOradores : Message
    {
        /// <summary>
        /// 
        /// </summary>
        public List<Orador> speakers { get; private set; }
        
       /// <summary>
       /// Copnstrutor padrão para a mensagem que envia a lista de oradores para os participantes do Promicnet.
       /// </summary>
       /// <param name="session_sk"></param>
       /// <param name="speakers"></param>

        public ListaOradores(string session_sk, List<Orador> speakers)
        {
            action = "speaker_list";
            session_id = session_sk;
            this.speakers = speakers;
        }
    }
}

 