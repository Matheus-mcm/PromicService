using System.Collections.Generic;
using static PromicnetIntegration.TypesDTO;

namespace PromicnetWebsocket.Messages
{
    /// <summary>
    /// 
    /// </summary>
    public class ResultadoVotacao : Message
    {
        /// <summary>
        /// 
        /// </summary>
        public string result { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int vote_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string description { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string[] types_allowed { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int[] vote_count { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<VotoDTO> votes { get; set; }
    }
}
