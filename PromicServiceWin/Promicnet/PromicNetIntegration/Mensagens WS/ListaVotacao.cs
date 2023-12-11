using PromicnetIntegration.Types;
using System.Collections.Generic;
using static PromicnetIntegration.TypesDTO;

namespace PromicnetWebsocket.Messages
{
    /// <summary>
    /// 
    /// </summary>
    public class ListaVotacao : Message
    {
        /// <summary>
        /// 
        /// </summary>
        public List<VotoDTO> votes { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="session_sk"></param>
        /// <param name="votos"></param>
        public ListaVotacao(string session_sk, List<VotoDTO> votos)
        {
            this.action = "vote_list";
            this.session_id = session_sk;
            votes = votos;
        }
    }
}
